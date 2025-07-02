using Dapper;
using Npgsql;
using RRC_Sender.Entities;
using RRC_Sender.Services.Enums;
using RRC_Sender.Services.Kafka;

namespace RRC_Sender.Services.BackGroundServices;

public class KafkaOutboxSender(NpgsqlDataSource dataSource) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);

        const string selectInfoNewOrderQuery = """
                                               SELECT id, name, amount, token 
                                               FROM items.orders_outbox 
                                               WHERE status = @status
                                               FOR UPDATE
                                               LIMIT 50
                                               """;
        
        const string updateStatusQuery = """
                                         UPDATE items.orders_outbox 
                                         SET status = @status 
                                         WHERE id = @id
                                         """;
        
        while (!cancellationToken.IsCancellationRequested)
        {
            var orderList = await connection.QueryFirstOrDefaultAsync<List<Order>>(selectInfoNewOrderQuery, new {status = OutboxStatus.NotSend});

            if (orderList is null || orderList.Count <= 0)
            {
                await Task.Delay(5000, cancellationToken);
                continue;   
            }

            foreach (var order in orderList)
            {
                await KafkaProducer.SendMessage(order);
                await connection.ExecuteAsync(updateStatusQuery, new { status = OutboxStatus.Sent, id = order.Id });
            }
            
            await Task.Delay(1000, cancellationToken);
        }
    }
}