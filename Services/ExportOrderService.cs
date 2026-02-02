using WarehouseX.DTOs;
using WarehouseX.Repositories;

namespace WarehouseX.Services
{
    public class ExportOrderService : IExportOrderService
    {
        private readonly IExportOrderRepository _repo;
        public ExportOrderService(IExportOrderRepository repo)
        {
            _repo = repo;
        }
        public Task<IEnumerable<ExportOrderDTO>> GetAllAsync() => _repo.GetAllAsync();
        public Task<ExportOrderDTO?> GetByIdAsync(int id) => _repo.GetByIdAsync(id);
        public Task AddAsync(ExportOrderDTO order) => _repo.AddAsync(order);
        public Task UpdateAsync(ExportOrderDTO order) => _repo.UpdateAsync(order);
        public Task DeleteAsync(int id) => _repo.DeleteAsync(id);
    }
}
