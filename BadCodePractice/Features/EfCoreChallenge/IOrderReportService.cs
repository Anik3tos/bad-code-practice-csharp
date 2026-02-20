namespace BadCodePractice.Features.EfCoreChallenge;

public interface IOrderReportService
{
    string Name { get; }
    Task<List<OrderReportDto>> GetOrderReportAsync(string city, CancellationToken cancellationToken = default);
}
