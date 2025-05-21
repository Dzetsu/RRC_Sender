using Microsoft.AspNetCore.Mvc;
using RRC_Sender.Entities;
using RRC_Sender.Services;

namespace RRC_Sender.Controllers;

[ApiController]
[Route("[controller]")]
public class OrderItemsController(IOrderService orderService) : ControllerBase
{
    [HttpGet]
    public Task<IEnumerable<Item>> GetAllItems()
    {
        return orderService.GetAll(CancellationToken.None);
    }

    [HttpPost]
    [Route("username")]
    public Task OrderItems(string username, [FromBody] string nameItem, long amount)
    {
        return orderService.CreateOrder(username, nameItem, amount, CancellationToken.None);
    }
}