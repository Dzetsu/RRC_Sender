using Dapper;
using Npgsql;
using RRC_Sender.Entities;
using RRC_Sender.Enums;
using RRC_Sender.Services.Kafka;

namespace RRC_Sender.Services.BackgroundServices;

public class KafkaOutboxSender(NpgsqlDataSource dataSource, KafkaProducer producer) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        
        const string selectInfoNewOrderQuery = """
                                               SELECT id, name, amount, token 
                                               FROM items.orders_outbox 
                                               WHERE status = 0
                                               """;
        
        const string updateStatusQuery = """
                                         UPDATE items.orders_outbox 
                                         SET status = @status 
                                         WHERE id = @id
                                         """;
        
        while (!cancellationToken.IsCancellationRequested)
        {
            var order = await connection.QueryFirstOrDefaultAsync<Order>(selectInfoNewOrderQuery);

            if (order == null || order.Id == 0)
            {
                await Task.Delay(5000, cancellationToken);
                continue;
            }
            
            await producer.SendMessage(order);
            await connection.ExecuteAsync(updateStatusQuery, new { status = OutboxStatus.Sent, id = order.Id });
            
            
            await Task.Delay(1000, cancellationToken);
        }
    }
}