using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using WarehouseX.DTOs;

namespace WarehouseX.Services
{
    public class ProductHistoryService
    {
        private readonly WarehouseXDbContext _context;
        private readonly string _connectionString;
        public ProductHistoryService(WarehouseXDbContext context, IConfiguration config)
        {
            _context = context;
            _connectionString = config.GetConnectionString("DefaultConnection")!;
        }

        public async Task<(List<ImportHistoryRow> imports, List<ExportHistoryRow> exports)> GetProductHistoryAsync(int productId)
        {
            var imports = new List<ImportHistoryRow>();
            var exports = new List<ExportHistoryRow>();
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            // Import history
            var importCmd = conn.CreateCommand();
            importCmd.CommandText = @"SELECT io.ImportDate, io.SupplierName, io.Description, ioi.Quantity
FROM ImportOrderItems ioi
JOIN ImportOrders io ON ioi.ImportOrderId = io.ImportOrderId
WHERE ioi.ProductId = @ProductId";
            importCmd.Parameters.Add(new SqlParameter("@ProductId", productId));
            using (var reader = await importCmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    imports.Add(new ImportHistoryRow
                    {
                        ImportDate = reader.GetDateTime(0),
                        SupplierName = reader.GetString(1),
                        Description = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                        Quantity = reader.GetInt32(3)
                    });
                }
            }

            // Export history
            var exportCmd = conn.CreateCommand();
            exportCmd.CommandText = @"SELECT eo.ExportDate, eo.CustomerName, eo.Description, eoi.Quantity
FROM ExportOrderItems eoi
JOIN ExportOrders eo ON eoi.ExportOrderId = eo.ExportOrderId
WHERE eoi.ProductId = @ProductId";
            exportCmd.Parameters.Add(new SqlParameter("@ProductId", productId));
            using (var reader = await exportCmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    exports.Add(new ExportHistoryRow
                    {
                        ExportDate = reader.GetDateTime(0),
                        CustomerName = reader.GetString(1),
                        Description = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                        Quantity = reader.GetInt32(3)
                    });
                }
            }
            return (imports, exports);
        }

        public async Task<List<ProductTransactionRow>> GetProductTransactionHistoryAsync(int productId, int initialStock)
        {
            var transactions = new List<ProductTransactionRow>();
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            // Import events
            var importCmd = conn.CreateCommand();
            importCmd.CommandText = @"SELECT io.ImportDate, io.SupplierName, ioi.Quantity, io.Description
FROM ImportOrderItems ioi
JOIN ImportOrders io ON ioi.ImportOrderId = io.ImportOrderId
WHERE ioi.ProductId = @ProductId";
            importCmd.Parameters.Add(new SqlParameter("@ProductId", productId));
            using (var reader = await importCmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    transactions.Add(new ProductTransactionRow
                    {
                        Date = reader.GetDateTime(0),
                        Type = "Import",
                        Partner = reader.GetString(1),
                        Quantity = reader.GetInt32(2),
                        Description = reader.IsDBNull(3) ? string.Empty : reader.GetString(3)
                    });
                }
            }

            // Export events
            var exportCmd = conn.CreateCommand();
            exportCmd.CommandText = @"SELECT eo.ExportDate, eo.CustomerName, eoi.Quantity, eo.Description
FROM ExportOrderItems eoi
JOIN ExportOrders eo ON eoi.ExportOrderId = eo.ExportOrderId
WHERE eoi.ProductId = @ProductId";
            exportCmd.Parameters.Add(new SqlParameter("@ProductId", productId));
            using (var reader = await exportCmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    transactions.Add(new ProductTransactionRow
                    {
                        Date = reader.GetDateTime(0),
                        Type = "Export",
                        Partner = reader.GetString(1),
                        Quantity = -reader.GetInt32(2), // Negative for export
                        Description = reader.IsDBNull(3) ? string.Empty : reader.GetString(3)
                    });
                }
            }

            // Sort by date
            transactions = transactions.OrderBy(t => t.Date).ToList();

            // Calculate running remaining stock
            int stock = initialStock;
            foreach (var t in transactions)
            {
                stock += t.Quantity;
                t.RemainingStock = stock;
            }
            return transactions;
        }
    }
}

public class ImportHistoryRow
{
    public DateTime ImportDate { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; }
}

public class ExportHistoryRow
{
    public DateTime ExportDate { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; }
}

public class ProductTransactionRow
{
    public DateTime Date { get; set; }
    public string Type { get; set; } = string.Empty; // "Import" or "Export"
    public string Partner { get; set; } = string.Empty; // Supplier or Customer
    public int Quantity { get; set; } // Positive for import, negative for export
    public string Description { get; set; } = string.Empty;
    public int RemainingStock { get; set; }
}
