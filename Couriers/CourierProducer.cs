using System.Text.Json.Serialization;
using Confluent.Kafka;
using System.Text.Json;

namespace KafkaProducer;

public class CourierProducer
{
    public async Task SendMessage(string token, string mes)
    {
        var config = new ProducerConfig()
        {
            BootstrapServers = "localhost:9092"
        };
        
        using var producer = new ProducerBuilder<Null, string>(config).Build();
        
        var order = new {Token = token, Message = mes};
        string message = JsonSerializer.Serialize(order);
        
        await producer.ProduceAsync(new TopicPartition("storage-consumer", new Partition(0)), new Message<Null, string> { Value = message });
        
        
        //await producer.ProduceAsync("courier-ok", new Message<Null, string> { Value = message });
    }
}