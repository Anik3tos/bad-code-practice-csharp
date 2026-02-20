using BadCodePractice.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace BadCodePractice.Data;

public sealed class ChallengeDbContext(DbContextOptions<ChallengeDbContext> options) : DbContext(options)
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Shipment> Shipments => Set<Shipment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.Property(x => x.Name).HasMaxLength(120);
            entity.Property(x => x.City).HasMaxLength(80);
            entity.HasIndex(x => x.City);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.Property(x => x.Name).HasMaxLength(120);
            entity.Property(x => x.Price).HasPrecision(10, 2);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasIndex(x => x.CustomerId);
            entity.HasOne(x => x.Customer)
                .WithMany(x => x.Orders)
                .HasForeignKey(x => x.CustomerId);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasIndex(x => x.OrderId);
            entity.HasIndex(x => x.ProductId);
            entity.HasOne(x => x.Order)
                .WithMany(x => x.OrderItems)
                .HasForeignKey(x => x.OrderId);
            entity.HasOne(x => x.Product)
                .WithMany(x => x.OrderItems)
                .HasForeignKey(x => x.ProductId);
        });

        modelBuilder.Entity<Shipment>(entity =>
        {
            entity.HasIndex(x => new { x.OrderId, x.ShippedAt });
            entity.HasOne(x => x.Order)
                .WithMany(x => x.Shipments)
                .HasForeignKey(x => x.OrderId);
        });
    }
}
