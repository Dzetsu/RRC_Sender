namespace Storage.Entities;

public class Order
{
    public long Id { get; set; }
    public string Name { get; set; }
    public long Amount { get; set; }
    public string Token { get; set; }
}