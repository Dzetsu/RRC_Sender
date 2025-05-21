using System.Text.Json;
using Confluent.Kafka;

namespace Storage;

public class StorageProducer
{
    public async Task Message(string token, char mes)
    {
        var bootstrapServers = "localhost:9092";
        
        var config = new ProducerConfig()
        {
            BootstrapServers = bootstrapServers
        };
        
        using var producer = new ProducerBuilder<Null, string>(config).Build();
        
        var value = new {Token = token, Answer = mes};
        string message = JsonSerializer.Serialize(value);
        
        await producer.ProduceAsync("mainProgCons", new Message<Null, string> { Value = message });
    }
}