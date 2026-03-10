const DB_NAME = 'ChunkFlowLogs';
const STORE_NAME = 'ApiLogs';
const CHUNK_SIZE_BYTES = 4 * 1024;
const BATCH_SIZE = 1000;
const TOTAL_LOGS = 1000;
const LEVELS = ['Debug', 'Info', 'Warning', 'Error'];

function openDb() {
  return new Promise((resolve, reject) => {
    const req = indexedDB.open(DB_NAME, 1);
    req.onerror = () => reject(req.error);
    req.onsuccess = () => resolve(req.result);
    req.onupgradeneeded = (e) => {
      const db = e.target.result;
      if (!db.objectStoreNames.contains(STORE_NAME)) {
        db.createObjectStore(STORE_NAME, { keyPath: 'id' });
      }
    };
  });
}

function clearStore(db) {
  return new Promise((resolve, reject) => {
    const tx = db.transaction(STORE_NAME, 'readwrite');
    tx.objectStore(STORE_NAME).clear();
    tx.oncomplete = () => resolve();
    tx.onerror = () => reject(tx.error);
  });
}

function addBatch(db, logs) {
  return new Promise((resolve, reject) => {
    const tx = db.transaction(STORE_NAME, 'readwrite');
    const store = tx.objectStore(STORE_NAME);
    for (const log of logs) store.put(log);
    tx.oncomplete = () => resolve();
    tx.onerror = () => reject(tx.error);
  });
}

function createLog(id, timestamp, level, message, durationMs) {
  return { id, timestamp, level, message, durationMs };
}

function randomLevel() {
  return LEVELS[Math.floor(Math.random() * LEVELS.length)];
}

function randomMessage() {
  const parts = ['Request', 'Response', 'Cache', 'DB', 'Auth', 'Sync', 'Job', 'Queue'];
  const p = parts[Math.floor(Math.random() * parts.length)];
  return `${p} completed in ${Math.floor(Math.random() * 500)}ms`;
}

function generateBatch(offset) {
  const batch = [];
  const baseTime = new Date().toISOString();
  const limit = Math.min(BATCH_SIZE, TOTAL_LOGS - offset);
  for (let i = 0; i < limit; i++) {
    const idx = offset + i;
    batch.push(createLog(
      `log-${idx}`,
      baseTime,
      randomLevel(),
      randomMessage(),
      Math.floor(Math.random() * 5000)
    ));
  }
  return batch;
}

export async function generateLogs(progressCallback) {
  const db = await openDb();
  await clearStore(db);
  let stored = 0;
  for (let offset = 0; offset < TOTAL_LOGS; offset += BATCH_SIZE) {
    const batch = generateBatch(offset);
    await addBatch(db, batch);
    stored += batch.length;
    if (progressCallback) progressCallback(stored, TOTAL_LOGS);
  }
  db.close();
  return stored;
}

function logToNdjsonLine(log) {
  return JSON.stringify(log) + '\n';
}

export async function streamLogsToApi(apiBaseUrl, requestId, onProgress) {
  const url = `${apiBaseUrl.replace(/\/$/, '')}/api/logs/stream/${requestId}`;
  const db = await openDb();
  const encoder = new TextEncoder();
  let buffer = '';
  let count = 0;

  const stream = new ReadableStream({
    start(controller) {
      const tx = db.transaction(STORE_NAME, 'readonly');
      const store = tx.objectStore(STORE_NAME);
      const req = store.openCursor();

      function processCursor(e) {
        const cursor = e.target.result;
        if (!cursor) {
          if (buffer.length) controller.enqueue(encoder.encode(buffer));
          controller.close();
          db.close();
          return;
        }
        buffer += logToNdjsonLine(cursor.value);
        count++;
        if (onProgress) onProgress(count);
        while (buffer.length >= CHUNK_SIZE_BYTES) {
          const chunk = buffer.slice(0, CHUNK_SIZE_BYTES);
          buffer = buffer.slice(CHUNK_SIZE_BYTES);
          controller.enqueue(encoder.encode(chunk));
        }
        cursor.continue();
      }

      req.onsuccess = processCursor;
      req.onerror = () => {
        controller.error(req.error);
        db.close();
      };
    }
  });

  const response = await fetch(url, {
    method: 'POST',
    headers: { 'Content-Type': 'application/x-ndjson' },
    body: stream,
    duplex: 'half'
  });
  if (!response.ok) throw new Error(`Stream failed: ${response.status}`);
  return count;
}

export async function getLogCount() {
  const db = await openDb();
  return new Promise((resolve, reject) => {
    const tx = db.transaction(STORE_NAME, 'readonly');
    const req = tx.objectStore(STORE_NAME).count();
    req.onsuccess = () => {
      db.close();
      resolve(req.result);
    };
    req.onerror = () => reject(req.error);
  });
}
