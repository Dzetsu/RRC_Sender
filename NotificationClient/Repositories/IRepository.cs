namespace NotificationClient.Repositories;

public interface IRepository<T> where T : class
{
    Task<long> GetId(T message);
    Task AddOrderMessage(T message);
}