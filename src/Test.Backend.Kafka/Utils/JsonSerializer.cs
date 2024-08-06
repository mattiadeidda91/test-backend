using Confluent.Kafka;
using System.Text;
using System.Text.Json;

namespace Test.Backend.Kafka.Utils
{
    public class JsonSerializer<T> :
        ISerializer<T>,
        IDeserializer<T>
    {
        public T Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
        {
            return JsonSerializer.Deserialize<T>(data)!;
        }

        public byte[] Serialize(T data, SerializationContext context)
        {
            return Encoding.UTF8.GetBytes(JsonSerializer.Serialize(data));
        }
    }
}
