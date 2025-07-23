using NotificationClient.Entities;

namespace NotificationClient.Services;

public interface INotificationService
{
    Task AddOrderMessage(TelegramMessage message);
    Task<bool> GetAnswer(TelegramMessage message);
}