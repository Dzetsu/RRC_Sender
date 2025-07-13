using NotificationClient.Repositories;

namespace NotificationClient.Services;

public class NotificationService(INotificationRepository repository) : INotificationService
{
    public async Task AddOrderMessage(TelegramMessage message)
    {
        await repository.AddOrderMessage(message);
    }

    public async Task<bool> GetAnswer(TelegramMessage message)
    {
        var id = await repository.GetId(message);

        return id == 0;
    }
}