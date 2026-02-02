namespace WarehouseX.Domain.Entities;

public class Inventory
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid WarehouseId { get; set; }
    public Guid ProductId { get; set; }

    public int OnHand { get; set; }
    public int Reserved { get; set; }

    // Concurrency (performans + doğruluk için önemli)
    public byte[] RowVersion { get; set; } = default!;
}
