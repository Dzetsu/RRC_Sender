using System.Text.Json;
using Confluent.Kafka;

namespace StorageChecker;

public class CheckerCouriers
{
    public async Task Message(string city)
    {
        var config = new ProducerConfig()
        {
            BootstrapServers = $"localhost:9092"
        };
        
        using var producer = new ProducerBuilder<Null, string>(config).Build();
        
        var checker = new {city = city};
        string message = JsonSerializer.Serialize(checker);
        
        await producer.ProduceAsync("checker", new Message<Null, string> {Value = message});
    }
}