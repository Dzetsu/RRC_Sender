using Storage.Entities;
using Storage.Repositories;

namespace Storage.Services;

public class StorageService(IStorageRepository repository) : IStorageService
{
    public async Task<bool> GetAnswer(Order order)
    {
        await repository.Update(order);
        var amount = await repository.Get(order);

        if (amount < 0)
        {
            order.Amount *= -1;
            await repository.Update(order);
        }
        else
        {
            return true;
        }
        
        order.Amount *= -1;
        return false;
    }
}