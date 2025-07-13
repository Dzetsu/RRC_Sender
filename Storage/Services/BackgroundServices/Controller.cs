using System.Diagnostics;
using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Storage.Entities;
using Storage.Services.Kafka;
using Storage.Settings;

namespace Storage.Services.BackgroundServices;

public class Controller(IOptions<KafkaConsumerSetting> consumerOptions, IStorageService service,
    StorageProducer producer, IOptions<KafkaProducerSetting> producerOptions) : BackgroundService
{
    private readonly KafkaConsumerSetting _consumerConfig = consumerOptions.Value ?? throw new ArgumentNullException(nameof(consumerOptions));
    private readonly KafkaProducerSetting _producerConfig = producerOptions.Value ?? throw new ArgumentNullException(nameof(producerOptions));
    
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var config = new ConsumerConfig
        {
            GroupId = _consumerConfig.GroupId,
            BootstrapServers = _consumerConfig.BootstrapServers,
            EnableAutoCommit = _consumerConfig.EnableAutoCommit,
            AutoOffsetReset = Enum.Parse<AutoOffsetReset>(_consumerConfig.AutoOffsetReset, true)
        };
        
        using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
        consumer.Subscribe(_consumerConfig.Topic);

        while (true)
        {
            try
            {
                var consumeResult = consumer.Consume(new TimeSpan(0, 0, 10));
                if (consumeResult == null) throw new TimeoutException("Timed out waiting for consumeResult");

                var order = JsonSerializer.Deserialize<Order>(consumeResult.Message.Value);
                if (order == null) throw new NullReferenceException("Message is null");

                var answer = await service.GetAnswer(order);
                
                ResultMessage resultMessage = new ResultMessage
                {
                    Token = order.Token
                };

                TelegramMessage telegramMessage = new TelegramMessage
                {
                    Id = order.Id,
                    Amount = order.Amount,
                    Name = order.Name
                };
                
                if (answer)
                {
                    await SendMessage(resultMessage, telegramMessage, '1', true);
                }
                else
                {
                    await SendMessage(resultMessage, telegramMessage, '2', false);
                    throw new Exception("Amount is negative");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
    }

    private async Task SendMessage(ResultMessage resultMessage, TelegramMessage telegramMessage, char answer, bool status)
    {
        resultMessage.Answer = answer;
        telegramMessage.Status = status;
        await producer.SendMessage(resultMessage, _producerConfig.MainProgramTopic);
        await producer.SendMessage(telegramMessage, _producerConfig.TelegramTopic);
    }
}