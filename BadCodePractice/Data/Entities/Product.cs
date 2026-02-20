namespace BadCodePractice.Data.Entities;

public sealed class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public List<OrderItem> OrderItems { get; set; } = new();
}
