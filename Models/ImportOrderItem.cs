namespace WarehouseX.Models
{
    public class ImportOrderItem
    {
        public int ImportOrderItemId { get; set; }
        public int ImportOrderId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public ImportOrder? ImportOrder { get; set; }
        public Product? Product { get; set; }
    }
}
