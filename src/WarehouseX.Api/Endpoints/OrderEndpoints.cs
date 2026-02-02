using Microsoft.EntityFrameworkCore;
using WarehouseX.Api.Contracts.Orders;
using WarehouseX.Domain;
using WarehouseX.Domain.Entities;
using WarehouseX.Infrastructure.Persistence;

namespace WarehouseX.Api.Endpoints;

public static class OrderEndpoints
{
    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/orders");

        group.MapPost("/", CreateOrder);
        group.MapGet("/", ListOrders);
        group.MapGet("/{id:guid}", GetOrderById);
        group.MapPost("/{id:guid}/pick", PickOrder);
        group.MapPost("/{id:guid}/cancel", CancelOrder);
        group.MapPost("/{id:guid}/ship", ShipOrder);

        return app;
    }

    private static async Task<IResult> CreateOrder(CreateOrderRequest request, WarehouseXDbContext db)
    {
        var errors = ValidateCreateOrder(request);
        if (errors.Count > 0)
        {
            return Results.ValidationProblem(errors);
        }

        var orderNumber = $"WX-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}".Substring(0, 32);

        await using var tx = await db.Database.BeginTransactionAsync();

        foreach (var item in request.Items)
        {
            var inventory = await db.Inventories
                .SingleOrDefaultAsync(x => x.WarehouseId == request.WarehouseId && x.ProductId == item.ProductId);

            if (inventory is null)
            {
                return Results.BadRequest(new { error = $"Inventory not found. ProductId={item.ProductId}" });
            }

            if (inventory.OnHand < item.Quantity)
            {
                return Results.BadRequest(new
                {
                    error = $"Insufficient stock. ProductId={item.ProductId}, OnHand={inventory.OnHand}"
                });
            }

            inventory.OnHand -= item.Quantity;
            inventory.Reserved += item.Quantity;
        }

        var order = new Order
        {
            OrderNumber = orderNumber,
            CustomerId = request.CustomerId,
            WarehouseId = request.WarehouseId,
            Status = OrderStatuses.Created,
            CreatedAt = DateTimeOffset.UtcNow,
            Items = request.Items.Select(i => new OrderItem
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            }).ToList()
        };

        db.Orders.Add(order);
        await db.SaveChangesAsync();
        await tx.CommitAsync();

        return Results.Created($"/orders/{order.Id}", new { order.Id, order.OrderNumber });
    }

    private static async Task<IResult> ListOrders(
        WarehouseXDbContext db,
        Guid? customerId,
        Guid? warehouseId,
        string? status,
        DateTimeOffset? from,
        DateTimeOffset? to,
        int page = 1,
        int pageSize = 20)
    {
        var query = db.Orders.AsNoTracking().AsQueryable();

        if (customerId.HasValue && customerId.Value != Guid.Empty)
        {
            query = query.Where(o => o.CustomerId == customerId.Value);
        }

        if (warehouseId.HasValue && warehouseId.Value != Guid.Empty)
        {
            query = query.Where(o => o.WarehouseId == warehouseId.Value);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(o => o.Status == status.Trim());
        }

        if (from.HasValue)
        {
            query = query.Where(o => o.CreatedAt >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(o => o.CreatedAt <= to.Value);
        }

        var normalizedPage = page < 1 ? 1 : page;
        var normalizedPageSize = Math.Clamp(pageSize, 1, 200);

        var pagePlusOne = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((normalizedPage - 1) * normalizedPageSize)
            .Take(normalizedPageSize + 1)
            .Select(o => new OrderListItemResponse(
                o.Id,
                o.OrderNumber,
                o.CustomerId,
                o.WarehouseId,
                o.Status,
                o.CreatedAt,
                o.Items.Count))
            .ToListAsync();

        var hasMore = pagePlusOne.Count > normalizedPageSize;
        if (hasMore)
        {
            pagePlusOne.RemoveAt(pagePlusOne.Count - 1);
        }

        return Results.Ok(new
        {
            page = normalizedPage,
            pageSize = normalizedPageSize,
            hasMore,
            items = pagePlusOne
        });
    }

    private static async Task<IResult> GetOrderById(WarehouseXDbContext db, Guid id)
    {
        var order = await db.Orders.AsNoTracking()
            .Include(o => o.Items)
            .SingleOrDefaultAsync(o => o.Id == id);

        if (order is null)
        {
            return Results.NotFound();
        }

        var response = new OrderDetailResponse(
            order.Id,
            order.OrderNumber,
            order.CustomerId,
            order.WarehouseId,
            order.Status,
            order.CreatedAt,
            order.Items.Select(i => new OrderItemResponse(i.ProductId, i.Quantity, i.UnitPrice)).ToList());

        return Results.Ok(response);
    }

    private static async Task<IResult> CancelOrder(WarehouseXDbContext db, Guid id)
    {
        var order = await db.Orders
            .Include(o => o.Items)
            .SingleOrDefaultAsync(o => o.Id == id);

        if (order is null)
        {
            return Results.NotFound();
        }

        if (order.Status == OrderStatuses.Cancelled)
        {
            return Results.Ok(new { order.Id, order.Status });
        }

        if (order.Status == OrderStatuses.Shipped)
        {
            return Results.Conflict(new { error = "Shipped orders cannot be cancelled." });
        }

        await using var tx = await db.Database.BeginTransactionAsync();

        foreach (var item in order.Items)
        {
            var inventory = await db.Inventories
                .SingleOrDefaultAsync(x => x.WarehouseId == order.WarehouseId && x.ProductId == item.ProductId);

            if (inventory is null)
            {
                return Results.Problem($"Inventory not found for ProductId={item.ProductId}.", statusCode: 500);
            }

            inventory.OnHand += item.Quantity;
            inventory.Reserved -= item.Quantity;
            if (inventory.Reserved < 0)
            {
                inventory.Reserved = 0;
            }
        }

        order.Status = OrderStatuses.Cancelled;
        await db.SaveChangesAsync();
        await tx.CommitAsync();

        return Results.Ok(new { order.Id, order.Status });
    }

    private static async Task<IResult> PickOrder(WarehouseXDbContext db, Guid id)
    {
        var order = await db.Orders.SingleOrDefaultAsync(o => o.Id == id);

        if (order is null)
        {
            return Results.NotFound();
        }

        if (order.Status == OrderStatuses.Picked)
        {
            return Results.Ok(new { order.Id, order.Status });
        }

        if (order.Status == OrderStatuses.Cancelled)
        {
            return Results.Conflict(new { error = "Cancelled orders cannot be picked." });
        }

        if (order.Status == OrderStatuses.Shipped)
        {
            return Results.Conflict(new { error = "Shipped orders cannot be picked." });
        }

        order.Status = OrderStatuses.Picked;
        await db.SaveChangesAsync();

        return Results.Ok(new { order.Id, order.Status });
    }

    private static async Task<IResult> ShipOrder(WarehouseXDbContext db, Guid id)
    {
        var order = await db.Orders
            .Include(o => o.Items)
            .SingleOrDefaultAsync(o => o.Id == id);

        if (order is null)
        {
            return Results.NotFound();
        }

        if (order.Status == OrderStatuses.Shipped)
        {
            return Results.Ok(new { order.Id, order.Status });
        }

        if (order.Status == OrderStatuses.Cancelled)
        {
            return Results.Conflict(new { error = "Cancelled orders cannot be shipped." });
        }

        if (order.Status != OrderStatuses.Created && order.Status != OrderStatuses.Picked)
        {
            return Results.Conflict(new { error = $"Order cannot be shipped from status '{order.Status}'." });
        }

        await using var tx = await db.Database.BeginTransactionAsync();

        foreach (var item in order.Items)
        {
            var inventory = await db.Inventories
                .SingleOrDefaultAsync(x => x.WarehouseId == order.WarehouseId && x.ProductId == item.ProductId);

            if (inventory is null)
            {
                return Results.Problem($"Inventory not found for ProductId={item.ProductId}.", statusCode: 500);
            }

            if (inventory.Reserved < item.Quantity)
            {
                return Results.Conflict(new
                {
                    error = $"Reserved stock insufficient. ProductId={item.ProductId}, Reserved={inventory.Reserved}"
                });
            }

            inventory.Reserved -= item.Quantity;
        }

        order.Status = OrderStatuses.Shipped;
        await db.SaveChangesAsync();
        await tx.CommitAsync();

        return Results.Ok(new { order.Id, order.Status });
    }

    private static Dictionary<string, string[]> ValidateCreateOrder(CreateOrderRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (request.CustomerId == Guid.Empty)
        {
            errors["customerId"] = new[] { "CustomerId is required." };
        }

        if (request.WarehouseId == Guid.Empty)
        {
            errors["warehouseId"] = new[] { "WarehouseId is required." };
        }

        if (request.Items is null || request.Items.Count == 0)
        {
            errors["items"] = new[] { "At least one item is required." };
            return errors;
        }

        var itemErrors = new List<string>();
        for (var i = 0; i < request.Items.Count; i++)
        {
            var item = request.Items[i];
            if (item.ProductId == Guid.Empty)
            {
                itemErrors.Add($"items[{i}].productId is required.");
            }

            if (item.Quantity <= 0)
            {
                itemErrors.Add($"items[{i}].quantity must be greater than zero.");
            }

            if (item.UnitPrice < 0)
            {
                itemErrors.Add($"items[{i}].unitPrice must be zero or greater.");
            }
        }

        if (itemErrors.Count > 0)
        {
            errors["items"] = itemErrors.ToArray();
        }

        return errors;
    }
}
