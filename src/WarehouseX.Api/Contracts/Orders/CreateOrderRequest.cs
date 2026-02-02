namespace WarehouseX.Api.Contracts.Orders;

public sealed record CreateOrderRequest(
    Guid CustomerId,
    Guid WarehouseId,
    List<CreateOrderItemRequest> Items
);

public sealed record CreateOrderItemRequest(
    Guid ProductId,
    int Quantity,
    decimal UnitPrice
);
