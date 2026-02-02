namespace WarehouseX.Domain.Entities;

public class Order
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string OrderNumber { get; set; } = default!; // uniq
    public Guid CustomerId { get; set; }
    public Guid WarehouseId { get; set; }

    public string Status { get; set; } = OrderStatuses.Created; // basit string, sonra enum'a dönebilir
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public List<OrderItem> Items { get; set; } = new();
}
