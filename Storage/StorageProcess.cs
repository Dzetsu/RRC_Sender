using System.Diagnostics;
using System.Text.Json;
using Confluent.Kafka;
using Dapper;
using Npgsql;
using Storage.Entities;
using Storage.Kafka;

namespace Storage;

class StorageProcess()
{
    static async Task Main()
    {
        var bootstrapServers = "localhost:9092";
        var connectionString = "Host=localhost;Username=postgres;Password=postgres;Database=postgres";
        var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        var config = new ConsumerConfig
        {
            GroupId = "StorageConsumer",
            BootstrapServers = bootstrapServers,
            EnableAutoCommit = false,
            AutoOffsetReset = AutoOffsetReset.Latest
        };
        
        using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
        consumer.Subscribe("storageKafka");
        
        const string updateAmountQuery = "update storage.storage_items set amount = amount - @amount where name = @name";
        const string checkAmountQuery = "select amount from storage.storage_items where name = @name";
        
        while (true)
        {
            try
            {
                var consumeResult = consumer.Consume(new TimeSpan(0, 0, 10));
                
                if (consumeResult == null)
                    throw new TimeoutException("Timed out waiting for consumeResult");
                
                var message = JsonSerializer.Deserialize<Order>(consumeResult.Message.Value);

                if (message == null)
                    throw new NullReferenceException("Message is null");
                
                var transaction = await connection.BeginTransactionAsync();
                await connection.ExecuteAsync(updateAmountQuery, new {amount = message.Amount, name = message.Name}, transaction);
                var amountCheck = await connection.QuerySingleAsync<long>(checkAmountQuery, new {name = message.Name}, transaction);

                ResultMessage resultMessage = new ResultMessage
                {
                    Token = message.Token
                };

                TelegramMessage telegramMessage = new TelegramMessage
                {
                    Id = message.Id,
                    Amount = message.Amount,
                    Name = message.Name
                };

                if (amountCheck < 0)
                {
                    resultMessage.Answer = '2';
                    await StorageKafkaProducer.SendMessage(resultMessage, "mainProgCons");
                    await transaction.RollbackAsync();
                    throw new Exception("Amount is negative");
                }
                
                resultMessage.Answer = '1';
                await StorageKafkaProducer.SendMessage(resultMessage, "mainProgCons");
                await StorageKafkaProducer.SendMessage(telegramMessage, "TelegramBot");
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
    }
}