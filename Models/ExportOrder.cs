namespace WarehouseX.Models
{
    public class ExportOrder
    {
        public int ExportOrderId { get; set; }
        public DateTime ExportDate { get; set; }
        public required string Description { get; set; }
        public required string CustomerName { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public ICollection<ExportOrderItem> Items { get; set; } = new List<ExportOrderItem>();
    }
}
