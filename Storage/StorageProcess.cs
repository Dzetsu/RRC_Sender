using System.Diagnostics;
using System.Text.Json;
using Confluent.Kafka;
using Dapper;
using Npgsql;

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

        StorageProducerForTgKafka producerForTgKafka = new StorageProducerForTgKafka();
        StorageProducer producer = new StorageProducer();
        using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
        consumer.Subscribe("storageKafka");
        
        const string updateAmount = "update storage.\"storageItems\" set amount = amount - @amount where name = @name";
        const string checkAmount = "select amount from storage.\"storageItems\" where name = @name";
        
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
                await connection.ExecuteAsync(updateAmount, new {amount = message.Amount, name = message.Name}, transaction);
                var amountCheck = await connection.QuerySingleAsync<long>(checkAmount, new {name = message.Name}, transaction);
                if (amountCheck < 0)
                {
                    await transaction.RollbackAsync();
                    await producer.Message(message.Token, 'n');
                    throw new Exception("Amount is negative");
                }
                await transaction.CommitAsync();
                await producer.Message(message.Token, 'y');
                await producerForTgKafka.Message(message.Id, message.Amount, message.Name);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
    }
}

public class Order
{
    public long Id { get; set; }
    public string Name { get; set; }
    public long Amount { get; set; }
    public string Token { get; set; }
}

/*if (item != null)
            {
                if (item.Amount > 0)
                {
                    await connection.ExecuteAsync(insertStorageOutbox, new { city = item.City, token = item.Token });
                    
                    bool switcher = true;

                    while (switcher)
                    {
                        using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
                        consumer.Subscribe("storage-consumer");
                        var consumeResult = consumer.Consume(TimeSpan.FromSeconds(10));
                        
                        if (consumeResult == null)
                        {
                            Console.WriteLine("Нет сообщений (таймаут 10 сек)");
                            continue;
                        }
                        
                        var result = JsonSerializer.Deserialize<CourierMessage>(consumeResult.Message.Value);
                        string? checker = result?.Message;
                        
                        if (checker == "y")
                        {
                            var transaction = await connection.BeginTransactionAsync();
                            await connection.ExecuteAsync(updateAmount, new { orderamount = item.Amount, name = item.Name }, transaction);
                            StorageProducer storageProducer = new StorageProducer();
                            await storageProducer.Message(item.Token, "y");
                            await transaction.CommitAsync();
                        }
                        else if (checker == "n")
                        {
                            StorageProducer storageProducer = new StorageProducer();
                            await storageProducer.Message(item.Token, "n");
                            const string sql4 = "delete from storage.storageoutbox where token = @token";
                            await connection.ExecuteAsync(sql4, new { token = item.Token }); 
                        }

                        await Task.Delay(5000);
                    }
                }
                else
                {
                    StorageProducer storageProducer = new StorageProducer();
                    await storageProducer.Message(item.Token, "n");
                    throw new Exception("Amount < 0");
                }*/