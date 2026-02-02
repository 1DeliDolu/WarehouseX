using Microsoft.EntityFrameworkCore;
using WarehouseX.Domain.Entities;

namespace WarehouseX.Infrastructure.Persistence;

public class WarehouseXDbContext : DbContext
{
    public WarehouseXDbContext(DbContextOptions<WarehouseXDbContext> options) : base(options)
    {
    }

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Inventory> Inventories => Set<Inventory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Order>(b =>
        {
            b.ToTable("Orders");
            b.HasKey(x => x.Id);

            b.Property(x => x.OrderNumber).HasMaxLength(64).IsRequired();
            b.Property(x => x.Status).HasMaxLength(32).IsRequired();

            b.HasIndex(x => x.OrderNumber).IsUnique();
            b.HasIndex(x => x.CreatedAt);
            b.HasIndex(x => x.Status);
            b.HasIndex(x => x.CustomerId);
            b.HasIndex(x => x.WarehouseId);
            b.HasIndex(x => new { x.WarehouseId, x.CreatedAt, x.Id, x.Status, x.OrderNumber, x.CustomerId })
                .HasDatabaseName("IX_Orders_Warehouse_CreatedAt_Id_Cover");

            b.HasMany(x => x.Items)
                .WithOne(x => x.Order)
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OrderItem>(b =>
        {
            b.ToTable("OrderItems");
            b.HasKey(x => x.Id);

            b.Property(x => x.UnitPrice).HasPrecision(18, 2);

            b.HasIndex(x => x.OrderId);
            b.HasIndex(x => new { x.OrderId, x.ProductId });
        });

        modelBuilder.Entity<Inventory>(b =>
        {
            b.ToTable("Inventory");
            b.HasKey(x => x.Id);

            b.HasIndex(x => new { x.WarehouseId, x.ProductId }).IsUnique();

            b.Property(x => x.RowVersion).IsRowVersion();
        });
    }
}
