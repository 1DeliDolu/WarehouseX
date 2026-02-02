namespace WarehouseX.DTOs
{
    public class ImportOrderItemDTO
    {
        public int ImportOrderItemId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public string ProductName { get; set; } = string.Empty;
    }
}
