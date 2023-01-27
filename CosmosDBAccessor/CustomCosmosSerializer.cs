using System.Text;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace CosmosDBAccessor;

public class CustomCosmosSerializer : CosmosSerializer
{
    private readonly JsonSerializer _serializer;

    public CustomCosmosSerializer()
    {
        _serializer = new JsonSerializer
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            },
            TypeNameHandling = TypeNameHandling.Objects
        };
    }

    public override T FromStream<T>(Stream stream)
    {
        using (stream)
        {
            if (typeof(Stream).IsAssignableFrom(typeof(T)))
            {
                return (T)(object)stream;
            }

            using StreamReader sr = new(stream);
            using JsonTextReader jsonTextReader = new(sr);
            return _serializer.Deserialize<T>(jsonTextReader);
        }
    }

    public override Stream ToStream<T>(T input)
    {
        MemoryStream streamPayload = new();
        using StreamWriter streamWriter = new(streamPayload, encoding: Encoding.Default, bufferSize: 1024, leaveOpen: true);
        using JsonWriter writer = new JsonTextWriter(streamWriter);

        writer.Formatting = Formatting.None;
        _serializer.Serialize(writer, input);
        writer.Flush();
        streamWriter.Flush();

        streamPayload.Position = 0;
        return streamPayload;
    }
}