using System.Collections.Generic;

namespace WarehouseX.DTOs
{
    public class ExportOrderDTO
    {
        public int ExportOrderId { get; set; }
        public DateTime ExportDate { get; set; }
        public string Description { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public List<ExportOrderItemDTO> Items { get; set; } = new();
    }
}
