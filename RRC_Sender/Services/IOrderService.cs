using RRC_Sender.Entities;

namespace RRC_Sender.Services;

public interface IOrderService
{
    Task<IEnumerable<Item>> GetAll(CancellationToken cancellationToken);
    Task CreateOrder(string username, string name, long amount, CancellationToken cancellationToken);
}   