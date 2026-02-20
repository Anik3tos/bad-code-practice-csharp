using BadCodePractice.Data;
using Microsoft.EntityFrameworkCore;

namespace BadCodePractice.Features.EfCoreChallenge;

public sealed class BadOrderReportService(ChallengeDbContext dbContext) : IOrderReportService
{
    public string Name => "Bad query";

    public async Task<List<OrderReportDto>> GetOrderReportAsync(
        string city,
        CancellationToken cancellationToken = default)
    {
        var orders = await dbContext.Orders
            .Include(o => o.Customer)
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .ToListAsync(cancellationToken);

        var result = new List<OrderReportDto>();

        foreach (var order in orders.Where(o => o.Customer.City == city))
        {
            var shipment = await dbContext.Shipments
                .Where(s => s.OrderId == order.Id)
                .OrderByDescending(s => s.ShippedAt)
                .FirstOrDefaultAsync(cancellationToken);

            var total = order.OrderItems.Sum(oi => oi.Quantity * oi.Product.Price);

            result.Add(new OrderReportDto
            {
                OrderId = order.Id,
                Customer = order.Customer.Name,
                City = order.Customer.City,
                Total = total,
                Shipped = shipment?.ShippedAt
            });
        }

        return result.OrderByDescending(r => r.Total).ToList();
    }
}
