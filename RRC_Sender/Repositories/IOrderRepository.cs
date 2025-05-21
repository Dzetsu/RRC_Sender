using RRC_Sender.Entities;

namespace RRC_Sender.Repositories;

public interface IOrderRepository : IRepository<Item>
{
    Task CreateOrder(string username, string nameItem, long amount, string token, CancellationToken cancellationToken);
};