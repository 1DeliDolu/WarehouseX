using WarehouseX.DTOs;

namespace WarehouseX.Services
{
    public interface IExportOrderService
    {
        Task<IEnumerable<ExportOrderDTO>> GetAllAsync();
        Task<ExportOrderDTO?> GetByIdAsync(int id);
        Task AddAsync(ExportOrderDTO order);
        Task UpdateAsync(ExportOrderDTO order);
        Task DeleteAsync(int id);
    }
}
