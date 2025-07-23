using Dapper;
using NotificationClient.Entities;
using Npgsql;

namespace NotificationClient.Repositories;

public class NotificationRepository(NpgsqlDataSource dataSource) : INotificationRepository
{
    public async Task<long> GetId(TelegramMessage message)
    {
        var connection = await dataSource.OpenConnectionAsync();
        var selectIdQuery = "SELECT id FROM telegram_bot.telegram_messages where id = @id";
        return await connection.QuerySingleOrDefaultAsync<long>(selectIdQuery, new { message.Id });
    }

    public async Task AddOrderMessage(TelegramMessage message)
    {
        var connection = await dataSource.OpenConnectionAsync();
        var insertMessage = "insert into telegram_bot.telegram_messages (id, name_item, amount, status) values (@id, @name_item, @amount, @status)";
        await connection.ExecuteAsync(insertMessage, new {id = message.Id, name_item = message.Name, amount = message.Amount, status = message.Status});
    }
}