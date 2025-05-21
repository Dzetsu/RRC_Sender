using System.Text.Json;
using Confluent.Kafka;

namespace StorageChecker;

public class ProducerBack
{
    public async Task Message(string command)
    {
        var config = new ProducerConfig()
        {
            BootstrapServers = $"localhost:9092"
        };
        
        using var producer = new ProducerBuilder<Null, string>(config).Build();
        
        var checker = new {command = command};
        string message = JsonSerializer.Serialize(checker);
        
        await producer.ProduceAsync("storage-ok", new Message<Null, string> {Value = message});
    }
}