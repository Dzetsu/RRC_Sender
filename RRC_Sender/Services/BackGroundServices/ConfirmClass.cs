using System.Diagnostics;
using System.Text.Json;
using Confluent.Kafka;
using Dapper;
using Microsoft.Extensions.Options;
using Npgsql;
using RRC_Sender.Entities;

namespace RRC_Sender.Services.BackGroundServices;

public class ConfirmClass(NpgsqlDataSource dataSource, IOptions<KafkaSettings> kafkaOptions) : BackgroundService
{
    private readonly KafkaSettings _kafkaSettings = kafkaOptions.Value;
    
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        
        const string updateStatusY = "update items.itemsoutbox set status = 'y' where token = @token";
        const string updateStatusN = "update items.itemsoutbox set status = 'n' where token = @token";
        const string deleteInfoOrder = "delete from items.orderitems where token = @token";
        
        var config = new ConsumerConfig // Попробовал IOption
        {
            GroupId = _kafkaSettings.GroupId,
            BootstrapServers = _kafkaSettings.BootstrapServers,
            EnableAutoCommit = _kafkaSettings.EnableAutoCommit,
            AutoOffsetReset = Enum.Parse<AutoOffsetReset>(_kafkaSettings.AutoOffsetReset, true),
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
                    case 'y':
                        await connection.ExecuteAsync(updateStatusY, new {token = message.Token});
                        break;
                    case 'n':
                        var transaction = await connection.BeginTransactionAsync(cancellationToken);
                        await connection.ExecuteAsync(updateStatusN, new {token = message.Token}, transaction);
                        await connection.ExecuteAsync(deleteInfoOrder, new {token = message.Token}, transaction);
                        await transaction.CommitAsync(cancellationToken);
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