using System.Net.Http.Json;

namespace WarehouseX.Tests.Integration;

public sealed class OrdersInventoryTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client;

    public OrdersInventoryTests(ApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task AdjustInventory_ThenGet_ReturnsUpdatedStock()
    {
        var warehouseId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        var adjustResponse = await _client.PostAsJsonAsync("/inventory/adjust", new
        {
            warehouseId,
            productId,
            deltaOnHand = 100
        });

        adjustResponse.EnsureSuccessStatusCode();

        var inventory = await _client.GetFromJsonAsync<InventoryResponse>(
            $"/inventory?warehouseId={warehouseId}&productId={productId}");

        Assert.NotNull(inventory);
        Assert.Equal(warehouseId, inventory!.WarehouseId);
        Assert.Equal(productId, inventory.ProductId);
        Assert.Equal(100, inventory.OnHand);
        Assert.Equal(0, inventory.Reserved);
    }

    [Fact]
    public async Task CreateOrder_ConsumesInventory_AndListReturnsOrder()
    {
        var warehouseId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        var seedResponse = await _client.PostAsJsonAsync("/inventory/adjust", new
        {
            warehouseId,
            productId,
            deltaOnHand = 10
        });

        seedResponse.EnsureSuccessStatusCode();

        var createResponse = await _client.PostAsJsonAsync("/orders", new
        {
            customerId,
            warehouseId,
            items = new[]
            {
                new { productId, quantity = 2, unitPrice = 10.50m }
            }
        });

        createResponse.EnsureSuccessStatusCode();

        var created = await createResponse.Content.ReadFromJsonAsync<OrderCreatedResponse>();
        Assert.NotNull(created);
        Assert.NotEqual(Guid.Empty, created!.Id);
        Assert.False(string.IsNullOrWhiteSpace(created.OrderNumber));

        var inventory = await _client.GetFromJsonAsync<InventoryResponse>(
            $"/inventory?warehouseId={warehouseId}&productId={productId}");

        Assert.NotNull(inventory);
        Assert.Equal(8, inventory!.OnHand);
        Assert.Equal(2, inventory.Reserved);

        var list = await _client.GetFromJsonAsync<OrderListResponse>("/orders?page=1&pageSize=20");
        Assert.NotNull(list);
        Assert.Contains(list!.Items, item => item.Id == created.Id && item.ItemCount == 1);

        var detail = await _client.GetFromJsonAsync<OrderDetailResponse>($"/orders/{created.Id}");
        Assert.NotNull(detail);
        Assert.Equal(customerId, detail!.CustomerId);
        Assert.Equal(warehouseId, detail.WarehouseId);
        Assert.Single(detail.Items);

        var cancelResponse = await _client.PostAsync($"/orders/{created.Id}/cancel", null);
        cancelResponse.EnsureSuccessStatusCode();

        var inventoryAfterCancel = await _client.GetFromJsonAsync<InventoryResponse>(
            $"/inventory?warehouseId={warehouseId}&productId={productId}");

        Assert.NotNull(inventoryAfterCancel);
        Assert.Equal(10, inventoryAfterCancel!.OnHand);
        Assert.Equal(0, inventoryAfterCancel.Reserved);

        var cancelledOrder = await _client.GetFromJsonAsync<OrderDetailResponse>($"/orders/{created.Id}");
        Assert.NotNull(cancelledOrder);
        Assert.Equal("Cancelled", cancelledOrder!.Status);
    }

    [Fact]
    public async Task PickThenShip_UpdatesStatusAndReserved()
    {
        var warehouseId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        var seedResponse = await _client.PostAsJsonAsync("/inventory/adjust", new
        {
            warehouseId,
            productId,
            deltaOnHand = 10
        });

        seedResponse.EnsureSuccessStatusCode();

        var createResponse = await _client.PostAsJsonAsync("/orders", new
        {
            customerId,
            warehouseId,
            items = new[]
            {
                new { productId, quantity = 3, unitPrice = 12.00m }
            }
        });

        createResponse.EnsureSuccessStatusCode();

        var created = await createResponse.Content.ReadFromJsonAsync<OrderCreatedResponse>();
        Assert.NotNull(created);

        var pickResponse = await _client.PostAsync($"/orders/{created!.Id}/pick", null);
        pickResponse.EnsureSuccessStatusCode();

        var pickedOrder = await _client.GetFromJsonAsync<OrderDetailResponse>($"/orders/{created.Id}");
        Assert.NotNull(pickedOrder);
        Assert.Equal("Picked", pickedOrder!.Status);

        var shipResponse = await _client.PostAsync($"/orders/{created.Id}/ship", null);
        shipResponse.EnsureSuccessStatusCode();

        var shippedOrder = await _client.GetFromJsonAsync<OrderDetailResponse>($"/orders/{created.Id}");
        Assert.NotNull(shippedOrder);
        Assert.Equal("Shipped", shippedOrder!.Status);

        var inventory = await _client.GetFromJsonAsync<InventoryResponse>(
            $"/inventory?warehouseId={warehouseId}&productId={productId}");

        Assert.NotNull(inventory);
        Assert.Equal(7, inventory!.OnHand);
        Assert.Equal(0, inventory.Reserved);
    }

    private sealed record InventoryResponse(Guid WarehouseId, Guid ProductId, int OnHand, int Reserved);

    private sealed record OrderCreatedResponse(Guid Id, string OrderNumber);

    private sealed record OrderListResponse(int Page, int PageSize, bool HasMore, List<OrderListItem> Items);

    private sealed record OrderListItem(
        Guid Id,
        string OrderNumber,
        Guid CustomerId,
        Guid WarehouseId,
        string Status,
        DateTimeOffset CreatedAt,
        int ItemCount);

    private sealed record OrderDetailResponse(
        Guid Id,
        string OrderNumber,
        Guid CustomerId,
        Guid WarehouseId,
        string Status,
        DateTimeOffset CreatedAt,
        List<OrderItem> Items);

    private sealed record OrderItem(Guid ProductId, int Quantity, decimal UnitPrice);
}
