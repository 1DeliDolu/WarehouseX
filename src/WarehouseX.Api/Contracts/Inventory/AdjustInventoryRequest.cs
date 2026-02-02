namespace WarehouseX.Api.Contracts.Inventory;

public sealed record AdjustInventoryRequest(
    Guid WarehouseId,
    Guid ProductId,
    int DeltaOnHand
);
