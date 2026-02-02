using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarehouseX.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(WarehouseXDbContext))]
    [Migration("20260202203000_AddKeysetCoveringIndex")]
    public partial class AddKeysetCoveringIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Orders_Warehouse_CreatedAt_Cover",
                table: "Orders");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_Warehouse_CreatedAt_Id_Cover",
                table: "Orders",
                columns: new[] { "WarehouseId", "CreatedAt", "Id", "Status", "OrderNumber", "CustomerId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Orders_Warehouse_CreatedAt_Id_Cover",
                table: "Orders");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_Warehouse_CreatedAt_Cover",
                table: "Orders",
                columns: new[] { "WarehouseId", "CreatedAt", "Status", "OrderNumber", "CustomerId" });
        }
    }
}
