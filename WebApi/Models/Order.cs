namespace WebApi.Models;

public class Order
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public string Description { get; set; } = string.Empty;

    public int Amount { get; set; }

    public bool IsPaid { get; set; }

    public DateTime CreatedAt { get; set; }
}
