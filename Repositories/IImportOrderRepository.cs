using WarehouseX.DTOs;

namespace WarehouseX.Repositories
{
    public interface IImportOrderRepository
    {
        Task<IEnumerable<ImportOrderDTO>> GetAllAsync();
        Task<ImportOrderDTO?> GetByIdAsync(int id);
        Task AddAsync(ImportOrderDTO order);
        Task UpdateAsync(ImportOrderDTO order);
        Task DeleteAsync(int id);
    }
}
