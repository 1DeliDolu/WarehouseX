using WarehouseX.DTOs;

namespace WarehouseX.Services
{
    public interface IImportOrderService
    {
        Task<IEnumerable<ImportOrderDTO>> GetAllAsync();
        Task<ImportOrderDTO?> GetByIdAsync(int id);
        Task AddAsync(ImportOrderDTO order);
        Task UpdateAsync(ImportOrderDTO order);
        Task DeleteAsync(int id);
    }
}
