using ECommerce.Monolith.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Monolith.Api.Data;

/// <summary>
/// The single EF Core database context for the whole monolith.
/// In Phase 1 every table lives in one SQL Server database — this is
/// deliberate, because the monolith baseline uses one relational store.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Product configuration.
        modelBuilder.Entity<Product>(p =>
        {
            p.Property(x => x.Name).IsRequired().HasMaxLength(200);
            p.Property(x => x.Description).HasMaxLength(2000);
            p.Property(x => x.Category).HasMaxLength(100);
            // decimal(18,2) is the standard money shape for SQL Server.
            p.Property(x => x.Price).HasColumnType("decimal(18,2)");

            // One product has exactly one inventory record.
            p.HasOne(x => x.Inventory)
             .WithOne(i => i.Product!)
             .HasForeignKey<InventoryItem>(i => i.ProductId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // Inventory: one row per product (enforced with a unique index).
        modelBuilder.Entity<InventoryItem>(i =>
        {
            i.HasIndex(x => x.ProductId).IsUnique();
        });

        // Order configuration.
        modelBuilder.Entity<Order>(o =>
        {
            o.Property(x => x.CustomerEmail).IsRequired().HasMaxLength(256);
            o.Property(x => x.TotalAmount).HasColumnType("decimal(18,2)");
            // Store the enum as a readable string ("Confirmed") instead of an int.
            o.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);

            o.HasMany(x => x.Items)
             .WithOne(x => x.Order!)
             .HasForeignKey(x => x.OrderId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // Order item configuration.
        modelBuilder.Entity<OrderItem>(oi =>
        {
            oi.Property(x => x.ProductName).IsRequired().HasMaxLength(200);
            oi.Property(x => x.UnitPrice).HasColumnType("decimal(18,2)");
        });
    }
}
