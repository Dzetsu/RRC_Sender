using System.Text.Json;
using Confluent.Kafka;

namespace Storage;

public class StorageProducerForTgKafka
{
    public async Task Message(long id, long amount, string name)
    {
        var bootstrapServers = "localhost:9092";
        
        var config = new ProducerConfig()
        {
            BootstrapServers = bootstrapServers
        };
        
        using var producer = new ProducerBuilder<Null, string>(config).Build();
        
        var value = new {Id = id, Amount = amount, Name = name};
        string message = JsonSerializer.Serialize(value);

        await producer.ProduceAsync("TelegramBot", new Message<Null, string> { Value = message });
    }
}