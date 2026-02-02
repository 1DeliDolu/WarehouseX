using System.Collections.Generic;

namespace WarehouseX.DTOs
{
    public class ImportOrderDTO
    {
        public int ImportOrderId { get; set; }
        public DateTime ImportDate { get; set; }
        public string Description { get; set; } = string.Empty;
        public string SupplierName { get; set; } = string.Empty;
        public List<ImportOrderItemDTO> Items { get; set; } = new();
    }
}
