using System.Linq.Expressions;
using System.Text.Json;
using System.Transactions;
using Confluent.Kafka;
using Dapper;
using Npgsql;
using StorageChecker;

class Program
{
    private const string connectionString = "Host=localhost;Username=postgres;Password=postgres;Database=postgres";
    private const string BootstrapServers = "localhost:9092";

    [Obsolete("Obsolete")]
    static async Task Main()
    {
        var orderConsumerConfig = new ConsumerConfig
        {
            GroupId = "storage-service-group",
            BootstrapServers = BootstrapServers,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };

        var courierConsumerConfig = new ConsumerConfig
        {
            BootstrapServers = BootstrapServers,
            GroupId = "storage-service-courier-group",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };

        using var orderConsumer = new ConsumerBuilder<Ignore, string>(orderConsumerConfig).Build();
        using var courierConsumer = new ConsumerBuilder<Ignore, string>(courierConsumerConfig).Build();

        orderConsumer.Subscribe("order");
        courierConsumer.Subscribe("courier-ok");

        CheckerCouriers producer = new CheckerCouriers();
        ProducerBack producerBack = new ProducerBack();

        try
        {
            while (true) 
            {
                var orderResult = orderConsumer.Consume(CancellationToken.None);
                var order = JsonSerializer.Deserialize<JsonElement>(orderResult.Value);

                var itemName = order.GetProperty("name").GetString();
                var itemAmount = order.GetProperty("amount").GetInt64();
                var city = order.GetProperty("city").GetString();

                using var connection = new NpgsqlConnection(connectionString);

                await connection.OpenAsync();
                await using var transaction = await connection.BeginTransactionAsync();

                bool flag = city != null;
                
                if (flag)
                {
                    try
                    {
                        const string sql =
                            "SELECT amount from storage.storage_items where name = @itemName and amount >= @itemAmount for update";

                        var amountItem =
                            await connection.QuerySingleAsync<long>(sql, new { itemName, itemAmount }, transaction);

                        if (amountItem <= 0)
                        {
                            throw new Exception($"Недостаточно товара {itemName} на складе");
                        }

                        const string sql2 =
                            "update storage.storage_items set amount = amount - @itemAmount where name = @itemName";

                        await connection.ExecuteAsync(sql2, new { itemName, itemAmount }, transaction);

                        await producer.Message(city);

                        var msg = courierConsumer.Consume();
                        

                        if (msg.Topic == "courier-ok" && msg.Message.Value == "yes")
                        {
                            await transaction.CommitAsync();
                            await producerBack.Message("yes");
                            orderConsumer.Commit(orderResult);
                            Console.WriteLine($"Конец в Storage: {DateTime.Now}");
                        }
                        else if (msg.Topic == "courier-ok" && msg.Message.Value == "no")
                        {
                            await transaction.RollbackAsync();
                            await producerBack.Message("no");
                            throw new Exception("Курьер недоступен, заказ отменен");
                        }
                    }
                    catch (Exception ex)
                    {
                        await producerBack.Message("no");
                        Console.WriteLine($"Ошибка обработки заказа: {ex.Message}");
                    }
                }
            }
        }
        finally
        {
            orderConsumer.Close();
            courierConsumer.Close();
        }
    }
}

