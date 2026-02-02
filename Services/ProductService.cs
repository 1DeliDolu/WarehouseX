using WarehouseX.DTOs;
using WarehouseX.Repositories;

namespace WarehouseX.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repo;
        public ProductService(IProductRepository repo)
        {
            _repo = repo;
        }
        public Task<IEnumerable<ProductDTO>> GetAllAsync() => _repo.GetAllAsync();
        public Task<ProductDTO?> GetByIdAsync(int id) => _repo.GetByIdAsync(id);
        public Task AddAsync(ProductDTO product) => _repo.AddAsync(product);
        public Task UpdateAsync(ProductDTO product) => _repo.UpdateAsync(product);
        public Task DeleteAsync(int id) => _repo.DeleteAsync(id);
    }
}
