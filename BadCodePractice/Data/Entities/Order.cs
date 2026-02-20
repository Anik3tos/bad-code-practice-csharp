namespace BadCodePractice.Data.Entities;

public sealed class Order
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public DateTime CreatedAt { get; set; }
    public Customer Customer { get; set; } = null!;
    public List<OrderItem> OrderItems { get; set; } = new();
    public List<Shipment> Shipments { get; set; } = new();
}
