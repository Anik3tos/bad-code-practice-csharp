using BadCodePractice.Data;
using Microsoft.EntityFrameworkCore;

namespace BadCodePractice.Features.EfCoreChallenge;

public sealed class RefactoredOrderReportService(ChallengeDbContext dbContext) : IOrderReportService
{
    public string Name => "Refactored query";

    public Task<List<OrderReportDto>> GetOrderReportAsync(
        string city,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Orders
            .AsNoTracking()
            .Where(o => o.Customer.City == city)
            .Select(o => new OrderReportDto
            {
                OrderId = o.Id,
                Customer = o.Customer.Name,
                City = o.Customer.City,
                Total = o.OrderItems.Sum(oi => oi.Quantity * oi.Product.Price),
                Shipped = o.Shipments
                    .OrderByDescending(s => s.ShippedAt)
                    .Select(s => (DateTime?)s.ShippedAt)
                    .FirstOrDefault()
            })
            .OrderByDescending(r => r.Total)
            .ToListAsync(cancellationToken);
    }
}
