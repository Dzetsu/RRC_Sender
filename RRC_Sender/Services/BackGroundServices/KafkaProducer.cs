using System.Text.Json;
using Confluent.Kafka;
using RRC_Sender.Entities;

namespace RRC_Sender.Services.BackGroundServices;

public class KafkaProducer
{
    public async Task SendKafkaMessage(Order order)
    {
        var bootstrapServers = "localhost:9092";

        var config = new ProducerConfig()
        {
            BootstrapServers = bootstrapServers
        };
        
        using var producer = new ProducerBuilder<Null, string>(config).Build();
        string message = JsonSerializer.Serialize(order);
        
        await producer.ProduceAsync("storageKafka", new Message<Null, string> { Value = message });
    }
}