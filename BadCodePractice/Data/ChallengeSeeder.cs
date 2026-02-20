using BadCodePractice.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace BadCodePractice.Data;

public static class ChallengeSeeder
{
    public static IReadOnlyList<string> Cities { get; } = new[]
    {
        "Seattle",
        "Austin",
        "Chicago",
        "Miami",
        "Berlin",
        "Tokyo",
        "Sofia",
        "London"
    };

    public static async Task SeedAsync(ChallengeDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await dbContext.Database.EnsureCreatedAsync(cancellationToken);

        if (await dbContext.Orders.AnyAsync(cancellationToken))
        {
            return;
        }

        var random = new Random(4242);

        var products = Enumerable.Range(1, 80)
            .Select(index => new Product
            {
                Name = $"Product {index:000}",
                Price = decimal.Round((decimal)(random.NextDouble() * 190 + 10), 2)
            })
            .ToList();

        var customers = Enumerable.Range(1, 450)
            .Select(index => new Customer
            {
                Name = $"Customer {index:0000}",
                City = Cities[random.Next(Cities.Count)]
            })
            .ToList();

        dbContext.Products.AddRange(products);
        dbContext.Customers.AddRange(customers);
        await dbContext.SaveChangesAsync(cancellationToken);

        var orders = new List<Order>(3000);
        var now = DateTime.UtcNow;
        for (var i = 0; i < 3000; i++)
        {
            var customer = customers[random.Next(customers.Count)];
            orders.Add(new Order
            {
                CustomerId = customer.Id,
                CreatedAt = now.AddDays(-random.Next(1, 365))
            });
        }

        dbContext.Orders.AddRange(orders);
        await dbContext.SaveChangesAsync(cancellationToken);

        var orderItems = new List<OrderItem>(orders.Count * 4);
        var shipments = new List<Shipment>(orders.Count);
        foreach (var order in orders)
        {
            var itemCount = random.Next(2, 7);
            for (var i = 0; i < itemCount; i++)
            {
                var product = products[random.Next(products.Count)];
                orderItems.Add(new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = product.Id,
                    Quantity = random.Next(1, 6)
                });
            }

            if (random.NextDouble() < 0.85)
            {
                var shipmentCount = random.Next(1, 4);
                for (var i = 0; i < shipmentCount; i++)
                {
                    shipments.Add(new Shipment
                    {
                        OrderId = order.Id,
                        ShippedAt = order.CreatedAt.AddDays(random.Next(1, 21) + i)
                    });
                }
            }
        }

        dbContext.OrderItems.AddRange(orderItems);
        dbContext.Shipments.AddRange(shipments);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
