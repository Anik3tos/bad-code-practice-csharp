namespace BadCodePractice.Data.Entities;

public sealed class Shipment
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public DateTime ShippedAt { get; set; }
    public Order Order { get; set; } = null!;
}
