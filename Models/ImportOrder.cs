namespace WarehouseX.Models
{
    public class ImportOrder
    {
        public int ImportOrderId { get; set; }
        public DateTime ImportDate { get; set; }
        public required string Description { get; set; }
        public required string SupplierName { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public ICollection<ImportOrderItem> Items { get; set; } = new List<ImportOrderItem>();
    }
}
