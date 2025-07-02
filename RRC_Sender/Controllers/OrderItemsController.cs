using Microsoft.AspNetCore.Mvc;
using RRC_Sender.Entities;
using RRC_Sender.Services;

namespace RRC_Sender.Controllers;

[ApiController]
[Route("[controller]")]
public class OrderItemsController(IOrderService orderService) : ControllerBase
{
    [HttpGet]
    public Task<IEnumerable<Item>> GetAll()
    {
        return orderService.GetAll(CancellationToken.None);
    }

    [HttpPost]
    public Task CreateOrder([FromQuery] string username, [FromBody] string nameItem, long amount)
    {
        return orderService.CreateOrder(username, nameItem, amount, CancellationToken.None);
    }
}