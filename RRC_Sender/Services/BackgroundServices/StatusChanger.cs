using System.Diagnostics;
using System.Text.Json;
using Confluent.Kafka;
using Dapper;
using Microsoft.Extensions.Options;
using Npgsql;
using RRC_Sender.Entities;
using RRC_Sender.Enums;
using RRC_Sender.Settings;

namespace RRC_Sender.Services.BackgroundServices;

public class StatusChanger(NpgsqlDataSource dataSource, IOptions<ConsumerKafkaSettings> kafkaOptions) : BackgroundService
{
    private readonly ConsumerKafkaSettings _consumerKafkaSetting = kafkaOptions.Value ?? throw new ArgumentNullException(nameof(kafkaOptions));
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        
        const string updateStatus = "update items.orders set status = @status where token = @token"; 
        
        var config = new ConsumerConfig 
        {
            GroupId = _consumerKafkaSetting.GroupId,
            BootstrapServers = _consumerKafkaSetting.BootstrapServers,
            EnableAutoCommit = _consumerKafkaSetting.EnableAutoCommit,
            AutoOffsetReset = Enum.Parse<AutoOffsetReset>(_consumerKafkaSetting.AutoOffsetReset, true)
        };
        
        using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
        consumer.Subscribe("mainProgCons");

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var consumeResult = consumer.Consume(new TimeSpan(0, 0, 10));
                
                if (consumeResult == null)
                    throw new TimeoutException("Timed out waiting for consumeResult");
                
                var message = JsonSerializer.Deserialize<Message>(consumeResult.Message.Value);

                if (message == null)
                    throw new NullReferenceException("Message is null");
                
                switch (message.Answer)
                {
                    case '1':
                        await connection.ExecuteAsync(updateStatus, new {status = OrderStatus.Confirmed, token = message.Token});
                        break;
                    case '2':
                        await connection.ExecuteAsync(updateStatus, new {status = OrderStatus.Cancelled, token = message.Token});
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
    }
}

public class Message
{
    public string Token { get; set; }
    public char Answer { get; set; }
}