namespace WarehouseX.Api.Contracts.Orders;

public sealed record OrderListItemResponse(
    Guid Id,
    string OrderNumber,
    Guid CustomerId,
    Guid WarehouseId,
    string Status,
    DateTimeOffset CreatedAt,
    int ItemCount
);

public sealed record OrderDetailResponse(
    Guid Id,
    string OrderNumber,
    Guid CustomerId,
    Guid WarehouseId,
    string Status,
    DateTimeOffset CreatedAt,
    List<OrderItemResponse> Items
);

public sealed record OrderItemResponse(
    Guid ProductId,
    int Quantity,
    decimal UnitPrice
);
