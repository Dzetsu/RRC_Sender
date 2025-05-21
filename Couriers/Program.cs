using Dapper;
using KafkaProducer;
using Npgsql;

namespace Couriers;

class Program
{
    static async Task Main()
    {
        var connectionString = "Host=localhost;Username=postgres;Password=postgres;Database=postgres";
        var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        
        var saverOrderToken = new HashSet<string>();
        
        while (true)
        {
            const string selectLockOrder = "select city, token from storage.storageoutbox where status = 'no'";
            const string selectUnlockId = "select id from couriers.courierlist where city = @city and status = 'unlock'";
            const string updateStatus = "update couriers.courierlist set status = 'lock' where id = @id and status = 'unlock'";
            var item = await connection.QueryFirstOrDefaultAsync<Item>(selectLockOrder);
            
            if (item != null)
            {
                if (!saverOrderToken.Contains(item.Token))
                {
                    var transaction = await connection.BeginTransactionAsync();
                    var id = await connection.QueryFirstOrDefaultAsync<long>(selectUnlockId, new { city = item.City },
                        transaction);

                    if (id > 0)
                    {
                        await connection.ExecuteAsync(updateStatus, new { id }, transaction);
                        CourierProducer courierProducer = new CourierProducer();
                        await courierProducer.SendMessage(item.Token, "y");
                        await transaction.CommitAsync();
                        saverOrderToken.Add(item.Token);
                    }
                    else
                    {
                        await transaction.RollbackAsync();
                        CourierProducer courierProducer = new CourierProducer();
                        await courierProducer.SendMessage(item.Token, "n");
                        throw new Exception("No courier");
                    }
                }
            }
        }
    }
}

class Item
{
    public string City { get; set; }
    public string Token { get; set; }
}