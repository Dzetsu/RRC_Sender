using Dapper;
using Npgsql;
using RRC_Sender.Entities;

namespace RRC_Sender.Services.BackGroundServices;

public class InfoSender(NpgsqlDataSource dataSource, KafkaProducer producerKafka) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);

        const string selectInfoNewOrder = @"SELECT id, name, amount, token 
                                            FROM items.itemsoutbox 
                                            WHERE status = '-';";
        
        const string updateStatus = @"UPDATE items.itemsoutbox 
                                      SET status = 's' 
                                      WHERE id = @id";
        
        while (!cancellationToken.IsCancellationRequested)
        {
            var transaction = await connection.BeginTransactionAsync(cancellationToken);
            var orderInfo = await connection.QueryFirstOrDefaultAsync<Order>(selectInfoNewOrder, transaction);

            if (orderInfo == null)
            {
                await transaction.RollbackAsync(cancellationToken);
                await Task.Delay(10000, cancellationToken);
                continue;   
            }
            
            await connection.ExecuteAsync(updateStatus, new { id = orderInfo.Id}, transaction);
            await transaction.CommitAsync(cancellationToken);
            
            await producerKafka.SendKafkaMessage(orderInfo);
            await Task.Delay(5000, cancellationToken);
        }
    }
}