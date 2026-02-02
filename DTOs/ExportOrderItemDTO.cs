namespace WarehouseX.DTOs
{
    public class ExportOrderItemDTO
    {
        public int ExportOrderItemId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public string ProductName { get; set; } = string.Empty;
    }
}
