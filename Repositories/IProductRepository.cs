using WarehouseX.DTOs;

namespace WarehouseX.Repositories
{
    public interface IProductRepository
    {
        Task<IEnumerable<ProductDTO>> GetAllAsync();
        Task<ProductDTO?> GetByIdAsync(int id);
        Task AddAsync(ProductDTO product);
        Task UpdateAsync(ProductDTO product);
        Task DeleteAsync(int id);
    }
}
