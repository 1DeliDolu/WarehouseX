using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarehouseX.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(WarehouseXDbContext))]
    [Migration("20260202200000_AddOrdersListCoveringIndex")]
    public partial class AddOrdersListCoveringIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Orders_Warehouse_CreatedAt_Cover",
                table: "Orders",
                columns: new[] { "WarehouseId", "CreatedAt", "Status", "OrderNumber", "CustomerId" });

            migrationBuilder.DropIndex(
                name: "IX_Orders_WarehouseId_CreatedAt",
                table: "Orders");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Orders_WarehouseId_CreatedAt",
                table: "Orders",
                columns: new[] { "WarehouseId", "CreatedAt" });

            migrationBuilder.DropIndex(
                name: "IX_Orders_Warehouse_CreatedAt_Cover",
                table: "Orders");
        }
    }
}
