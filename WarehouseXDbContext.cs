using Microsoft.EntityFrameworkCore;
using WarehouseX.Models;

namespace WarehouseX
{
    public class WarehouseXDbContext : DbContext
    {
        public WarehouseXDbContext(DbContextOptions<WarehouseXDbContext> options) : base(options) { }

        public DbSet<Product> Products { get; set; }
        public DbSet<ImportOrder> ImportOrders { get; set; }
        public DbSet<ImportOrderItem> ImportOrderItems { get; set; }
        public DbSet<ExportOrder> ExportOrders { get; set; }
        public DbSet<ExportOrderItem> ExportOrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>().HasData(
                new Product { ProductId = 1, Name = "Widget A", Description = "Standard widget", SKU = "WIDGET-A", QuantityInStock = 100 },
                new Product { ProductId = 2, Name = "Widget B", Description = "Advanced widget", SKU = "WIDGET-B", QuantityInStock = 50 },
                new Product { ProductId = 3, Name = "Gadget X", Description = "Multi-purpose gadget", SKU = "GADGET-X", QuantityInStock = 75 }
            );

            modelBuilder.Entity<ImportOrder>().HasData(
                new ImportOrder { ImportOrderId = 1, ImportDate = new DateTime(2025, 6, 1), SupplierName = "Acme Supplies", Description = "Initial stock" },
                new ImportOrder { ImportOrderId = 2, ImportDate = new DateTime(2025, 6, 10), SupplierName = "Widget World", Description = "Restock widgets" }
            );

            modelBuilder.Entity<ImportOrderItem>().HasData(
                new ImportOrderItem { ImportOrderItemId = 1, ImportOrderId = 1, ProductId = 1, Quantity = 60 },
                new ImportOrderItem { ImportOrderItemId = 2, ImportOrderId = 1, ProductId = 3, Quantity = 40 },
                new ImportOrderItem { ImportOrderItemId = 3, ImportOrderId = 2, ProductId = 1, Quantity = 40 },
                new ImportOrderItem { ImportOrderItemId = 4, ImportOrderId = 2, ProductId = 2, Quantity = 50 }
            );

            modelBuilder.Entity<ExportOrder>().HasData(
                new ExportOrder { ExportOrderId = 1, ExportDate = new DateTime(2025, 6, 15), CustomerName = "BestBuy", Description = "Order for BestBuy" },
                new ExportOrder { ExportOrderId = 2, ExportDate = new DateTime(2025, 6, 20), CustomerName = "GadgetMart", Description = "Order for GadgetMart" }
            );

            modelBuilder.Entity<ExportOrderItem>().HasData(
                new ExportOrderItem { ExportOrderItemId = 1, ExportOrderId = 1, ProductId = 1, Quantity = 30 },
                new ExportOrderItem { ExportOrderItemId = 2, ExportOrderId = 1, ProductId = 2, Quantity = 10 },
                new ExportOrderItem { ExportOrderItemId = 3, ExportOrderId = 2, ProductId = 3, Quantity = 20 }
            );

            // ...existing seeding code for other entities if needed
        }
    }
}
