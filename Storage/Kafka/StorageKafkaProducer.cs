using System.Text.Json;
using Confluent.Kafka;

namespace Storage.Kafka;

public static class StorageKafkaProducer
{
    public static async Task SendMessage<T>(T mes, string topic) where T : class
    {
        var bootstrapServers = "localhost:9092";
        
        var config = new ProducerConfig
        {
            BootstrapServers = bootstrapServers
        };
        
        using var producer = new ProducerBuilder<Null, string>(config).Build();
        string message = JsonSerializer.Serialize(mes);
        
        await producer.ProduceAsync(topic, new Message<Null, string> { Value = message });
    }
}