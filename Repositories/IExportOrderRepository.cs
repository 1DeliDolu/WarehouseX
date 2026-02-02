using WarehouseX.DTOs;

namespace WarehouseX.Repositories
{
    public interface IExportOrderRepository
    {
        Task<IEnumerable<ExportOrderDTO>> GetAllAsync();
        Task<ExportOrderDTO?> GetByIdAsync(int id);
        Task AddAsync(ExportOrderDTO order);
        Task UpdateAsync(ExportOrderDTO order);
        Task DeleteAsync(int id);
    }
}
