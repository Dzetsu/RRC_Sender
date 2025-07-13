using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using Storage.Settings;

namespace Storage.Services.Kafka;

public class StorageProducer(IOptions<KafkaProducerSetting> options)
{
    private readonly KafkaProducerSetting _consumerConfig = options.Value ?? throw new ArgumentNullException(nameof(options));

    public async Task SendMessage<T>(T mes, string topic) where T : class
    {
        var config = new ProducerConfig
        {
            BootstrapServers = _consumerConfig.BootstrapServers
        };
        
        using var producer = new ProducerBuilder<Null, string>(config).Build();
        string message = JsonSerializer.Serialize(mes);
        
        await producer.ProduceAsync(topic, new Message<Null, string> { Value = message });
    }
}