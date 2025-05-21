using System.Text.Json;
using Confluent.Kafka;
using Dapper;
using Npgsql;
using RRC_Sender.Entities;

namespace RRC_Sender.Repositories;

public class OrderRepository(NpgsqlDataSource dataSource) : IOrderRepository
{
    public async Task<IEnumerable<Item>> GetAll(CancellationToken cancellationToken)
    {
        var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        const string selectAll = "Select * from items.items";
        return await connection.QueryAsync<Item>(selectAll);
    }
    
    public async Task CreateOrder(string username, string nameItem, long amount, string token, CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);

        const string selectId = "SELECT id from items.Items where nameitem = @nameitem";
        const string insertInfoOutBox = "INSERT into items.itemsoutbox (username, name, amount, token) values (@username, @name, @amount, @token)";
        const string insertInfoOrder = "INSERT INTO items.orderitems (username, name, amount, token) values (@username, @name, @amount, @token)";
        
        var itemId = await connection.QueryFirstAsync<long>(selectId, new { nameitem = nameItem });

        if (itemId == 0)
            throw new Exception($"Item with name {nameItem} not found");

        var transaction = await connection.BeginTransactionAsync(cancellationToken);
        await connection.ExecuteAsync(insertInfoOutBox, new {username = username, name = nameItem,  amount, token}, transaction);
        await connection.ExecuteAsync(insertInfoOrder, new {username = username, name = nameItem, amount, token}, transaction);
        await transaction.CommitAsync(cancellationToken);
    }
}


/*while (switcher)
{
    var consumeResult = consumer.Consume(cancellationToken);
    var order = JsonSerializer.Deserialize<JsonElement>(consumeResult.Message.Value);
    string? orderCheck = order.GetProperty("Message").GetString();

    if (orderCheck == "y")
    {
        await connection.ExecuteAsync(insertIfTrue, new { item_id = itemId, amount,  username }, transaction);
        await transaction.CommitAsync(cancellationToken);
        switcher = false;
    }
    else if (orderCheck == "n")
    {
        await transaction.RollbackAsync(cancellationToken);
        await connection.ExecuteAsync(deleteIfFalse, new { token });
        switcher = false;
    }
}*/
