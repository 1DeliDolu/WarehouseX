using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using WarehouseX.Domain;
using WarehouseX.Domain.Entities;
using WarehouseX.Infrastructure.Persistence;

var options = SeedOptions.Parse(args);
if (options.ShowHelp)
{
    SeedOptions.PrintHelp();
    return;
}

var connectionString = options.ConnectionString ?? "Server=localhost;Port=3306;Database=warehousex;User=root;Password=";
var serverVersionText = options.ServerVersion ?? "8.4.0";
var serverVersion = ServerVersion.Parse(serverVersionText);

var dbOptions = new DbContextOptionsBuilder<WarehouseXDbContext>()
    .UseMySql(connectionString, serverVersion)
    .Options;

using var db = new WarehouseXDbContext(dbOptions);
await db.Database.MigrateAsync();

Console.WriteLine("Seeder started.");
Console.WriteLine($"ServerVersion: {serverVersionText}");
Console.WriteLine($"Warehouses: {options.Warehouses}, Products: {options.Products}, Customers: {options.Customers}, Orders: {options.Orders}");

var warehouseIds = Enumerable.Range(0, options.Warehouses).Select(_ => Guid.NewGuid()).ToArray();
var productIds = Enumerable.Range(0, options.Products).Select(_ => Guid.NewGuid()).ToArray();
var customerIds = Enumerable.Range(0, options.Customers).Select(_ => Guid.NewGuid()).ToArray();

if (!options.SkipInventory)
{
    if (await db.Inventories.AnyAsync())
    {
        Console.WriteLine("Inventory already has data. Skipping inventory seed.");
    }
    else
    {
        await SeedInventoryAsync(db, warehouseIds, productIds, options.BatchSize);
    }
}

if (!options.SkipOrders)
{
    if (await db.Orders.AnyAsync())
    {
        Console.WriteLine("Orders already have data. Skipping orders seed.");
    }
    else
    {
        await SeedOrdersAsync(db, warehouseIds, productIds, customerIds, options);
    }
}

if (options.WriteSampleIds)
{
    var outputPath = string.IsNullOrWhiteSpace(options.SampleIdsOutput)
        ? "seed_ids.json"
        : options.SampleIdsOutput;
    await WriteSampleIdsAsync(db, outputPath);
}

Console.WriteLine("Seeder finished.");

static async Task SeedInventoryAsync(
    WarehouseXDbContext db,
    IReadOnlyList<Guid> warehouseIds,
    IReadOnlyList<Guid> productIds,
    int batchSize)
{
    var total = warehouseIds.Count * productIds.Count;
    var created = 0;
    var random = new Random();
    var batch = new List<Inventory>(batchSize);
    var originalDetectChanges = db.ChangeTracker.AutoDetectChangesEnabled;
    db.ChangeTracker.AutoDetectChangesEnabled = false;

    try
    {
        foreach (var warehouseId in warehouseIds)
        {
            foreach (var productId in productIds)
            {
                var onHand = random.Next(50, 500);
                batch.Add(new Inventory
                {
                    WarehouseId = warehouseId,
                    ProductId = productId,
                    OnHand = onHand,
                    Reserved = 0
                });

                if (batch.Count >= batchSize)
                {
                    db.Inventories.AddRange(batch);
                    await db.SaveChangesAsync();
                    created += batch.Count;
                    batch.Clear();
                    Console.WriteLine($"Inventory: {created}/{total}");
                }
            }
        }

        if (batch.Count > 0)
        {
            db.Inventories.AddRange(batch);
            await db.SaveChangesAsync();
            created += batch.Count;
            Console.WriteLine($"Inventory: {created}/{total}");
        }
    }
    finally
    {
        db.ChangeTracker.AutoDetectChangesEnabled = originalDetectChanges;
    }
}

static async Task SeedOrdersAsync(
    WarehouseXDbContext db,
    IReadOnlyList<Guid> warehouseIds,
    IReadOnlyList<Guid> productIds,
    IReadOnlyList<Guid> customerIds,
    SeedOptions options)
{
    if (options.Orders <= 0)
    {
        return;
    }

    var random = new Random();
    var statuses = new[] { OrderStatuses.Created, OrderStatuses.Picked, OrderStatuses.Shipped, OrderStatuses.Cancelled };
    var batch = new List<Order>(options.BatchSize);
    var originalDetectChanges = db.ChangeTracker.AutoDetectChangesEnabled;
    db.ChangeTracker.AutoDetectChangesEnabled = false;
    var now = DateTimeOffset.UtcNow;

    try
    {
        for (var i = 1; i <= options.Orders; i++)
        {
            var createdAt = now.AddDays(-random.Next(0, options.DaysBack))
                .AddMinutes(-random.Next(0, 1440));

            var orderNumber = $"WX-{createdAt:yyyyMMddHHmmss}-{i:D8}";

            var itemCount = random.Next(1, options.MaxItemsPerOrder + 1);
            var items = new List<OrderItem>(itemCount);
            for (var j = 0; j < itemCount; j++)
            {
                var productId = productIds[random.Next(productIds.Count)];
                var quantity = random.Next(1, 6);
                var unitPrice = Math.Round((decimal)random.NextDouble() * 100m + 1m, 2);

                items.Add(new OrderItem
                {
                    ProductId = productId,
                    Quantity = quantity,
                    UnitPrice = unitPrice
                });
            }

            var order = new Order
            {
                OrderNumber = orderNumber,
                CustomerId = customerIds[random.Next(customerIds.Count)],
                WarehouseId = warehouseIds[random.Next(warehouseIds.Count)],
                Status = statuses[random.Next(statuses.Length)],
                CreatedAt = createdAt,
                Items = items
            };

            batch.Add(order);

            if (batch.Count >= options.BatchSize)
            {
                db.Orders.AddRange(batch);
                await db.SaveChangesAsync();
                Console.WriteLine($"Orders: {i}/{options.Orders}");
                batch.Clear();
            }
        }

        if (batch.Count > 0)
        {
            db.Orders.AddRange(batch);
            await db.SaveChangesAsync();
            Console.WriteLine($"Orders: {options.Orders}/{options.Orders}");
        }
    }
    finally
    {
        db.ChangeTracker.AutoDetectChangesEnabled = originalDetectChanges;
    }
}

static async Task WriteSampleIdsAsync(WarehouseXDbContext db, string outputPath)
{
    var inv = await db.Inventories.AsNoTracking()
        .Select(x => new { x.WarehouseId, x.ProductId })
        .FirstOrDefaultAsync();

    var ord = await db.Orders.AsNoTracking()
        .Select(x => new { x.Id, x.WarehouseId, x.CustomerId })
        .FirstOrDefaultAsync();

    if (inv is null && ord is null)
    {
        Console.WriteLine("No data found in Inventory/Orders. Skipping sample IDs output.");
        return;
    }

    var payload = new
    {
        warehouseId = inv?.WarehouseId ?? ord!.WarehouseId,
        productId = inv?.ProductId,
        customerId = ord?.CustomerId,
        orderId = ord?.Id
    };

    var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true });
    await File.WriteAllTextAsync(outputPath, json);
    Console.WriteLine($"Sample IDs written to {Path.GetFullPath(outputPath)}");
}

sealed record SeedOptions(
    int Warehouses,
    int Products,
    int Customers,
    int Orders,
    int MaxItemsPerOrder,
    int BatchSize,
    int DaysBack,
    string? ConnectionString,
    string? ServerVersion,
    bool SkipInventory,
    bool SkipOrders,
    bool WriteSampleIds,
    string? SampleIdsOutput,
    bool ShowHelp)
{
    public static SeedOptions Parse(string[] args)
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            if (!arg.StartsWith("-", StringComparison.Ordinal))
            {
                continue;
            }

            var trimmed = arg.TrimStart('-');
            if (string.IsNullOrWhiteSpace(trimmed))
            {
                continue;
            }

            var parts = trimmed.Split('=', 2, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 2)
            {
                map[parts[0]] = parts[1];
                continue;
            }

            if (i + 1 < args.Length && !args[i + 1].StartsWith("-", StringComparison.Ordinal))
            {
                map[trimmed] = args[i + 1];
                i++;
            }
            else
            {
                map[trimmed] = "true";
            }
        }

        bool showHelp = map.ContainsKey("help") || map.ContainsKey("h") || map.ContainsKey("?");

        var sampleIdsOutput = GetString(map, "ids-out");
        var writeSampleIds = GetBool(map, "write-ids") || !string.IsNullOrWhiteSpace(sampleIdsOutput);

        return new SeedOptions(
            Warehouses: GetInt(map, "warehouses", 5),
            Products: GetInt(map, "products", 1000),
            Customers: GetInt(map, "customers", 1000),
            Orders: GetInt(map, "orders", 10000),
            MaxItemsPerOrder: GetInt(map, "max-items", 5),
            BatchSize: GetInt(map, "batch", 500),
            DaysBack: GetInt(map, "days", 90),
            ConnectionString: GetString(map, "connection"),
            ServerVersion: GetString(map, "server-version"),
            SkipInventory: GetBool(map, "skip-inventory"),
            SkipOrders: GetBool(map, "skip-orders"),
            WriteSampleIds: writeSampleIds,
            SampleIdsOutput: sampleIdsOutput,
            ShowHelp: showHelp);
    }

    public static void PrintHelp()
    {
        Console.WriteLine("WarehouseX.Seeder usage:");
        Console.WriteLine("  dotnet run --project tools/WarehouseX.Seeder -- --orders 10000 --products 1000 --warehouses 5");
        Console.WriteLine("");
        Console.WriteLine("Options:");
        Console.WriteLine("  --connection       MySQL connection string (default: localhost:3306, user=root)");
        Console.WriteLine("  --server-version   MySQL server version (default: 8.4.0)");
        Console.WriteLine("  --warehouses       Number of warehouses (default: 5)");
        Console.WriteLine("  --products         Number of products (default: 1000)");
        Console.WriteLine("  --customers        Number of customers (default: 1000)");
        Console.WriteLine("  --orders           Number of orders (default: 10000)");
        Console.WriteLine("  --max-items        Max items per order (default: 5)");
        Console.WriteLine("  --batch            Batch size for inserts (default: 500)");
        Console.WriteLine("  --days             Spread orders across last N days (default: 90)");
        Console.WriteLine("  --skip-inventory   Skip inventory seeding");
        Console.WriteLine("  --skip-orders      Skip orders seeding");
        Console.WriteLine("  --write-ids        Write sample IDs to seed_ids.json");
        Console.WriteLine("  --ids-out          Output file for sample IDs (implies --write-ids)");
    }

    private static int GetInt(IReadOnlyDictionary<string, string> map, string key, int defaultValue)
    {
        return map.TryGetValue(key, out var value) && int.TryParse(value, out var parsed)
            ? parsed
            : defaultValue;
    }

    private static string? GetString(IReadOnlyDictionary<string, string> map, string key)
    {
        return map.TryGetValue(key, out var value) ? value : null;
    }

    private static bool GetBool(IReadOnlyDictionary<string, string> map, string key)
    {
        return map.TryGetValue(key, out var value) &&
               (string.Equals(value, "true", StringComparison.OrdinalIgnoreCase) || value == "1");
    }
}
