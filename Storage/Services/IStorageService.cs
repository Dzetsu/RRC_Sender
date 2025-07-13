using Storage.Entities;

namespace Storage.Services;

public interface IStorageService
{
    Task<bool> GetAnswer(Order order);
}