using Microsoft.EntityFrameworkCore;
using WarehouseX.DTOs;
using WarehouseX.Models;

namespace WarehouseX.Repositories
{
    public class ExportOrderRepository : IExportOrderRepository
    {
        private readonly WarehouseXDbContext _context;
        public ExportOrderRepository(WarehouseXDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ExportOrderDTO>> GetAllAsync()
        {
            var orders = await _context.ExportOrders
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .Select(o => new ExportOrderDTO
                {
                    ExportOrderId = o.ExportOrderId,
                    ExportDate = o.ExportDate,
                    Description = o.Description,
                    CustomerName = o.CustomerName,
                    Items = o.Items.Select(i => new ExportOrderItemDTO
                    {
                        ExportOrderItemId = i.ExportOrderItemId,
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

        public async Task<ExportOrderDTO?> GetByIdAsync(int id)
        {
            var o = await _context.ExportOrders
                .Include(x => x.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(x => x.ExportOrderId == id);
            if (o == null) return null;
            var dto = new ExportOrderDTO
            {
                ExportOrderId = o.ExportOrderId,
                ExportDate = o.ExportDate,
                Description = o.Description,
                CustomerName = o.CustomerName,
                Items = o.Items.Select(i => new ExportOrderItemDTO
                {
                    ExportOrderItemId = i.ExportOrderItemId,
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

        public async Task AddAsync(ExportOrderDTO order)
        {
            var entity = new ExportOrder
            {
                ExportDate = order.ExportDate,
                Description = order.Description,
                CustomerName = order.CustomerName,
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow,
                Items = order.Items.Select(i => new ExportOrderItem
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    Product = null!,
                    ExportOrder = null!
                }).ToList()
            };
            _context.ExportOrders.Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ExportOrderDTO order)
        {
            var entity = await _context.ExportOrders.Include(x => x.Items).FirstOrDefaultAsync(x => x.ExportOrderId == order.ExportOrderId);
            if (entity == null) return;
            entity.ExportDate = order.ExportDate;
            entity.Description = order.Description;
            entity.CustomerName = order.CustomerName;
            entity.ModifiedDate = DateTime.UtcNow;
            // For simplicity, remove and re-add items
            _context.ExportOrderItems.RemoveRange(entity.Items ?? new List<ExportOrderItem>());
            entity.Items = order.Items.Select(i => new ExportOrderItem
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                Product = null!,
                ExportOrder = null!
            }).ToList();
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.ExportOrders.Include(x => x.Items).FirstOrDefaultAsync(x => x.ExportOrderId == id);
            if (entity == null) return;
            _context.ExportOrderItems.RemoveRange(entity.Items ?? new List<ExportOrderItem>());
            _context.ExportOrders.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
