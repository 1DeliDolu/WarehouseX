using Microsoft.EntityFrameworkCore;
using WarehouseX.DTOs;
using WarehouseX.Models;

namespace WarehouseX.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly WarehouseXDbContext _context;
        public ProductRepository(WarehouseXDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ProductDTO>> GetAllAsync()
        {
            return await _context.Products
                .Select(p => new ProductDTO
                {
                    ProductId = p.ProductId,
                    Name = p.Name,
                    Description = p.Description,
                    SKU = p.SKU,
                    QuantityInStock = p.QuantityInStock
                })
                .ToListAsync();
        }

        public async Task<ProductDTO?> GetByIdAsync(int id)
        {
            var p = await _context.Products.FindAsync(id);
            if (p == null) return null;
            return new ProductDTO
            {
                ProductId = p.ProductId,
                Name = p.Name,
                Description = p.Description,
                SKU = p.SKU,
                QuantityInStock = p.QuantityInStock
            };
        }

        public async Task AddAsync(ProductDTO product)
        {
            var entity = new Product
            {
                Name = product.Name,
                Description = product.Description,
                SKU = product.SKU,
                QuantityInStock = product.QuantityInStock,
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow
            };
            _context.Products.Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ProductDTO product)
        {
            var entity = await _context.Products.FindAsync(product.ProductId);
            if (entity == null) return;
            entity.Name = product.Name;
            entity.Description = product.Description;
            entity.SKU = product.SKU;
            entity.QuantityInStock = product.QuantityInStock;
            entity.ModifiedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.Products.FindAsync(id);
            if (entity == null) return;
            _context.Products.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
