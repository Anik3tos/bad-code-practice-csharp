namespace BadCodePractice.Features.EfCoreChallenge;

public sealed class OrderReportDto
{
    public int OrderId { get; init; }
    public string Customer { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public decimal Total { get; init; }
    public DateTime? Shipped { get; init; }
}
