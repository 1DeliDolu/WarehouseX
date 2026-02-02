namespace WarehouseX.Models
{
    public class ExportOrderItem
    {
        public int ExportOrderItemId { get; set; }
        public int ExportOrderId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public ExportOrder? ExportOrder { get; set; }
        public Product? Product { get; set; }
    }
}
