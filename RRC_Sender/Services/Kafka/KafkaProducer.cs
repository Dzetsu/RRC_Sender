using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using RRC_Sender.Entities;
using RRC_Sender.Settings;

namespace RRC_Sender.Services.Kafka;

public class KafkaProducer(IOptions<ProducerKafkaSettings> options)
{
    private readonly ProducerKafkaSettings _producerConfig = options.Value ?? throw new ArgumentNullException(nameof(options));
    
    public async Task SendMessage(Order order)
    {
        var config = new ProducerConfig()
        {
            BootstrapServers = _producerConfig.BootstrapServers
        };
        
        using var producer = new ProducerBuilder<Null, string>(config).Build();
        string message = JsonSerializer.Serialize(order);
        
        await producer.ProduceAsync("storage", new Message<Null, string> { Value = message });
    }
}