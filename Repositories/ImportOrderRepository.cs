using Microsoft.EntityFrameworkCore;
using WarehouseX.DTOs;
using WarehouseX.Models;

namespace WarehouseX.Repositories
{
    public class ImportOrderRepository : IImportOrderRepository
    {
        private readonly WarehouseXDbContext _context;
        public ImportOrderRepository(WarehouseXDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ImportOrderDTO>> GetAllAsync()
        {
            var orders = await _context.ImportOrders
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .Select(o => new ImportOrderDTO
                {
                    ImportOrderId = o.ImportOrderId,
                    ImportDate = o.ImportDate,
                    Description = o.Description,
                    SupplierName = o.SupplierName,
                    Items = o.Items.Select(i => new ImportOrderItemDTO
                    {
                        ImportOrderItemId = i.ImportOrderItemId,
                        ProductId = i.ProductId,
                        Quantity = i.Quantity
                    }).ToList()
                })
                .ToListAsync();

            // Set ProductName in memory
            foreach (var order in orders)
            {
                foreach (var item in order.Items)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);
                    item.ProductName = product?.Name ?? string.Empty;
                }
            }
            return orders;
        }

        public async Task<ImportOrderDTO?> GetByIdAsync(int id)
        {
            var o = await _context.ImportOrders
                .Include(x => x.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(x => x.ImportOrderId == id);
            if (o == null) return null;
            var dto = new ImportOrderDTO
            {
                ImportOrderId = o.ImportOrderId,
                ImportDate = o.ImportDate,
                Description = o.Description,
                SupplierName = o.SupplierName,
                Items = o.Items.Select(i => new ImportOrderItemDTO
                {
                    ImportOrderItemId = i.ImportOrderItemId,
                    ProductId = i.ProductId,
                    Quantity = i.Quantity
                }).ToList()
            };
            foreach (var item in dto.Items)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                item.ProductName = product?.Name ?? string.Empty;
            }
            return dto;
        }

        public async Task AddAsync(ImportOrderDTO order)
        {
            var entity = new ImportOrder
            {
                ImportDate = order.ImportDate,
                Description = order.Description,
                SupplierName = order.SupplierName,
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow,
                Items = order.Items.Select(i => new ImportOrderItem
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    Product = null!,
                    ImportOrder = null!
                }).ToList()
            };
            _context.ImportOrders.Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ImportOrderDTO order)
        {
            var entity = await _context.ImportOrders.Include(x => x.Items).FirstOrDefaultAsync(x => x.ImportOrderId == order.ImportOrderId);
            if (entity == null) return;
            entity.ImportDate = order.ImportDate;
            entity.Description = order.Description;
            entity.SupplierName = order.SupplierName;
            entity.ModifiedDate = DateTime.UtcNow;
            // For simplicity, remove and re-add items
            _context.ImportOrderItems.RemoveRange(entity.Items);
            entity.Items = order.Items.Select(i => new ImportOrderItem
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                Product = null!,
                ImportOrder = null!
            }).ToList();
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.ImportOrders.Include(x => x.Items).FirstOrDefaultAsync(x => x.ImportOrderId == id);
            if (entity == null) return;
            _context.ImportOrderItems.RemoveRange(entity.Items);
            _context.ImportOrders.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
