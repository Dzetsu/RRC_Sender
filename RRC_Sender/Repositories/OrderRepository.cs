using System.Text.Json;
using Confluent.Kafka;
using Dapper;
using Npgsql;
using RRC_Sender.Entities;
using Exception = System.Exception;

namespace RRC_Sender.Repositories;

public class OrderRepository(NpgsqlDataSource dataSource) : IOrderRepository
{
    public async Task<IEnumerable<Item>> GetAll(CancellationToken cancellationToken)
    {
        var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        const string selectAll = "select * from items.items";
        return await connection.QueryAsync<Item>(selectAll);
    }
    
    public async Task CreateOrder(string username, string nameItem, long amount, string token, CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);

        const string selectIdQuery = "SELECT id FROM items.items WHERE nameitem = @nameitem";
        const string insertInfoOutBoxQuery = "INSERT INTO items.orders_outbox (username, name, amount, token) VALUES (@username, @name, @amount, @token)";
        const string insertInfoOrderQuery = "INSERT INTO items.orders (username, name, amount, token) VALUES (@username, @name, @amount, @token)";
        
        var itemId = await connection.QueryFirstAsync<long>(selectIdQuery, new { nameitem = nameItem });

        if (itemId == 0)
            throw new ArgumentException($"Item with name {nameItem} not found");

        var transaction = await connection.BeginTransactionAsync(cancellationToken);
        await connection.ExecuteAsync(insertInfoOutBoxQuery, new {username = username, name = nameItem,  amount, token}, transaction);
        await connection.ExecuteAsync(insertInfoOrderQuery, new {username = username, name = nameItem, amount, token}, transaction);
        await transaction.CommitAsync(cancellationToken);
    }
}
