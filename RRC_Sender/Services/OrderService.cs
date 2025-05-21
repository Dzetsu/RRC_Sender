using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using Confluent.Kafka;
using RRC_Sender.Entities;
using RRC_Sender.Repositories;
using RRC_Sender.Services.BusinessLogic;

namespace RRC_Sender.Services;

public class OrderService(IOrderRepository orderRepository) : IOrderService
{
    public async Task<IEnumerable<Item>> GetAll(CancellationToken cancellationToken = default)
    {
        return await orderRepository.GetAll(cancellationToken);
    }

    public async Task CreateOrder(string username, string nameItem, long amount, CancellationToken cancellationToken = default)
    {
        try
        {
            if (amount <= 0)
                throw new ArgumentException("Amount cannot be negative", nameof(amount));

            TokenGenerator generator = new TokenGenerator();
            string token = generator.Generate(username, nameItem, amount);
            await orderRepository.CreateOrder(username, nameItem, amount, token, cancellationToken);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }
    }
}