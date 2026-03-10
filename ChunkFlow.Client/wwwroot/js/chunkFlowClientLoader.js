window.chunkFlowClientReady = (async () => {
  const m = await import('./chunkFlowClient.js');
  window.ChunkFlowClient = {
    generateLogs: m.generateLogs,
    streamLogsToApi: m.streamLogsToApi,
    getLogCount: m.getLogCount
  };
})();
window.waitForChunkFlowClient = () => window.chunkFlowClientReady;
window.getChunkFlowQuery = () => {
  const p = new URLSearchParams(window.location.search);
  return { requestId: p.get('requestId') || '', apiUrl: p.get('apiUrl') || '' };
};
