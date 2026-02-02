using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WarehouseX.Migrations
{
    /// <inheritdoc />
    public partial class SeedAndNullabilityFixes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "ExportOrders",
                columns: new[] { "ExportOrderId", "CreatedDate", "CustomerName", "Description", "ExportDate", "ModifiedDate" },
                values: new object[,]
                {
                    { 1, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "BestBuy", "Order for BestBuy", new DateTime(2025, 6, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 2, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "GadgetMart", "Order for GadgetMart", new DateTime(2025, 6, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) }
                });

            migrationBuilder.InsertData(
                table: "ImportOrders",
                columns: new[] { "ImportOrderId", "CreatedDate", "Description", "ImportDate", "ModifiedDate", "SupplierName" },
                values: new object[,]
                {
                    { 1, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Initial stock", new DateTime(2025, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Acme Supplies" },
                    { 2, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Restock widgets", new DateTime(2025, 6, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Widget World" }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "ProductId", "CreatedDate", "Description", "ModifiedDate", "Name", "QuantityInStock", "SKU" },
                values: new object[,]
                {
                    { 1, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Standard widget", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Widget A", 100, "WIDGET-A" },
                    { 2, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Advanced widget", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Widget B", 50, "WIDGET-B" },
                    { 3, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Multi-purpose gadget", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Gadget X", 75, "GADGET-X" }
                });

            migrationBuilder.InsertData(
                table: "ExportOrderItems",
                columns: new[] { "ExportOrderItemId", "ExportOrderId", "ProductId", "Quantity" },
                values: new object[,]
                {
                    { 1, 1, 1, 30 },
                    { 2, 1, 2, 10 },
                    { 3, 2, 3, 20 }
                });

            migrationBuilder.InsertData(
                table: "ImportOrderItems",
                columns: new[] { "ImportOrderItemId", "ImportOrderId", "ProductId", "Quantity" },
                values: new object[,]
                {
                    { 1, 1, 1, 60 },
                    { 2, 1, 3, 40 },
                    { 3, 2, 1, 40 },
                    { 4, 2, 2, 50 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ExportOrderItems",
                keyColumn: "ExportOrderItemId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "ExportOrderItems",
                keyColumn: "ExportOrderItemId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "ExportOrderItems",
                keyColumn: "ExportOrderItemId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "ImportOrderItems",
                keyColumn: "ImportOrderItemId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "ImportOrderItems",
                keyColumn: "ImportOrderItemId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "ImportOrderItems",
                keyColumn: "ImportOrderItemId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "ImportOrderItems",
                keyColumn: "ImportOrderItemId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "ExportOrders",
                keyColumn: "ExportOrderId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "ExportOrders",
                keyColumn: "ExportOrderId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "ImportOrders",
                keyColumn: "ImportOrderId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "ImportOrders",
                keyColumn: "ImportOrderId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 3);
        }
    }
}
