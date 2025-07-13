using Dapper;
using Npgsql;
using Storage.Entities;

namespace Storage.Repositories;

public class StorageRepository(NpgsqlDataSource dataSource) : IStorageRepository
{
    public async Task Update(Order order)
    {
        var connection = await dataSource.OpenConnectionAsync();
        const string updateAmountQuery = "update storage.storage_items set amount = amount - @amount where name = @name";
        await connection.ExecuteAsync(updateAmountQuery, new { amount = order.Amount, name = order.Name });
    }
    
    public async Task<long> Get(Order order)
    {
        var connection = await dataSource.OpenConnectionAsync();
        const string checkAmountQuery = "select amount from storage.storage_items where name = @name";
        var storageAmount  = await connection.QuerySingleOrDefaultAsync<long>(checkAmountQuery, new { name = order.Name });
        return storageAmount;
    }
}