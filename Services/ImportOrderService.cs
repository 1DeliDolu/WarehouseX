using WarehouseX.DTOs;
using WarehouseX.Repositories;

namespace WarehouseX.Services
{
    public class ImportOrderService : IImportOrderService
    {
        private readonly IImportOrderRepository _repo;
        public ImportOrderService(IImportOrderRepository repo)
        {
            _repo = repo;
        }
        public Task<IEnumerable<ImportOrderDTO>> GetAllAsync() => _repo.GetAllAsync();
        public Task<ImportOrderDTO?> GetByIdAsync(int id) => _repo.GetByIdAsync(id);
        public Task AddAsync(ImportOrderDTO order) => _repo.AddAsync(order);
        public Task UpdateAsync(ImportOrderDTO order) => _repo.UpdateAsync(order);
        public Task DeleteAsync(int id) => _repo.DeleteAsync(id);
    }
}
