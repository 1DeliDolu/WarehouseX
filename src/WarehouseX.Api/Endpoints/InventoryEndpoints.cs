using Microsoft.EntityFrameworkCore;
using WarehouseX.Api.Contracts.Inventory;
using WarehouseX.Domain.Entities;
using WarehouseX.Infrastructure.Persistence;

namespace WarehouseX.Api.Endpoints;

public static class InventoryEndpoints
{
    public static IEndpointRouteBuilder MapInventoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/inventory");

        group.MapPost("/adjust", AdjustInventory);
        group.MapGet("/", GetInventory);

        return app;
    }

    private static async Task<IResult> AdjustInventory(AdjustInventoryRequest request, WarehouseXDbContext db)
    {
        if (request.WarehouseId == Guid.Empty || request.ProductId == Guid.Empty)
        {
            return Results.BadRequest(new { error = "WarehouseId and ProductId are required." });
        }

        if (request.DeltaOnHand == 0)
        {
            return Results.BadRequest(new { error = "DeltaOnHand cannot be zero." });
        }

        const int maxRetry = 2;

        for (var attempt = 0; attempt <= maxRetry; attempt++)
        {
            try
            {
                var inventory = await db.Inventories
                    .SingleOrDefaultAsync(x => x.WarehouseId == request.WarehouseId && x.ProductId == request.ProductId);

                if (inventory is null)
                {
                    inventory = new Inventory
                    {
                        WarehouseId = request.WarehouseId,
                        ProductId = request.ProductId,
                        OnHand = 0,
                        Reserved = 0
                    };

                    db.Inventories.Add(inventory);
                }

                var newOnHand = inventory.OnHand + request.DeltaOnHand;
                if (newOnHand < 0)
                {
                    return Results.BadRequest(new { error = $"OnHand cannot be negative. Current={inventory.OnHand}" });
                }

                inventory.OnHand = newOnHand;

                await db.SaveChangesAsync();
                return Results.Ok(new
                {
                    inventory.WarehouseId,
                    inventory.ProductId,
                    inventory.OnHand,
                    inventory.Reserved
                });
            }
            catch (DbUpdateConcurrencyException) when (attempt < maxRetry)
            {
                foreach (var entry in db.ChangeTracker.Entries())
                {
                    entry.State = EntityState.Detached;
                }
            }
        }

        return Results.Problem("Concurrency conflict. Please retry.", statusCode: 409);
    }

    private static async Task<IResult> GetInventory(WarehouseXDbContext db, Guid warehouseId, Guid productId)
    {
        if (warehouseId == Guid.Empty || productId == Guid.Empty)
        {
            return Results.BadRequest(new { error = "warehouseId and productId are required." });
        }

        var inventory = await db.Inventories.AsNoTracking()
            .SingleOrDefaultAsync(x => x.WarehouseId == warehouseId && x.ProductId == productId);

        return inventory is null
            ? Results.NotFound()
            : Results.Ok(new { inventory.WarehouseId, inventory.ProductId, inventory.OnHand, inventory.Reserved });
    }
}
