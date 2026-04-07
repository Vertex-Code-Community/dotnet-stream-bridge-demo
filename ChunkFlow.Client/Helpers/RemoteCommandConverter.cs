using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ChunkFlow.Shared.Models.Commands;
using System.Reflection;

namespace ChunkFlow.Client.Helpers;

public class RemoteCommandConverter : JsonConverter<RemoteCommand>
{
    private readonly Dictionary<string, Type> _dictionary = new();

    public RemoteCommandConverter()
    {
        foreach (Type item in from myType in Assembly.GetAssembly(typeof(RemoteCommand))!.GetTypes()
                              where myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(RemoteCommand))
                              select myType)
        {
            _dictionary[item.Name] = item;
        }
    }

    private Type? GetTypeByCommandType(string commandType)
    {
        if (_dictionary.TryGetValue(commandType, out var value)) return value;
        return null;
    }

    public override bool CanRead => true;

    public override void WriteJson(JsonWriter writer, RemoteCommand? value, JsonSerializer serializer)
    {
        if (value == null!) return;

        var originalToken = JToken.FromObject(value!);
        var original = (JObject)originalToken;

        if (value.NextCommand != null!)
        {
            original.Remove("NextCommand");
            original.Add(new JProperty("NextCommand", (JObject)JToken.FromObject(value.NextCommand, serializer)));
        }

        if (value.CancelCommand != null!)
        {
            original.Remove("CancelCommand");
            original.Add(new JProperty("CancelCommand", (JObject)JToken.FromObject(value.CancelCommand, serializer)));
        }

        var commandJObject = new JObject()
        {
            { "Type", new JValue(value.GetType().Name) },
            { "Params", new JObject(original) }
        };

        commandJObject.WriteTo(writer);
    }

    public override RemoteCommand? ReadJson(JsonReader reader, Type objectType, RemoteCommand? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        JObject jsonObject = JObject.Load(reader);

        var parameters = jsonObject.GetValue("Params") as JObject;
        var jTokenType = jsonObject.GetValue("Type");

        if (parameters is null || !parameters.HasValues) return null!;

        var nextCommandJObject = parameters.GetValue("NextCommand");
        var cancelCommandJObject = parameters.GetValue("CancelCommand");

        parameters.Remove("NextCommand");
        parameters.Remove("CancelCommand");

        var type = GetTypeByCommandType((string)(jTokenType as JValue)!.Value!);
        var obj = (RemoteCommand)JsonConvert.DeserializeObject(parameters.ToString(), type!)!;

        if (nextCommandJObject is not null && nextCommandJObject.HasValues)
        {
            var nextCommand = JsonConvert.DeserializeObject<RemoteCommand>(nextCommandJObject.ToString(), this);
            if (nextCommand != null!) obj.NextCommand = nextCommand;
        }

        if (cancelCommandJObject is not null && cancelCommandJObject.HasValues)
        {
            var cancelCommand = JsonConvert.DeserializeObject<RemoteCommand>(cancelCommandJObject.ToString(), this);
            if (cancelCommand != null!) obj.CancelCommand = cancelCommand;
        }

        return obj;
    }
}
