using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bai3_WebBanHang.Migrations
{
    /// <inheritdoc />
    public partial class UpdateOrderCascadeDeleteRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderStatusHistories_Orders_OrderId1",
                table: "OrderStatusHistories");

            migrationBuilder.DropIndex(
                name: "IX_OrderStatusHistories_OrderId1",
                table: "OrderStatusHistories");

            migrationBuilder.DropColumn(
                name: "OrderId1",
                table: "OrderStatusHistories");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OrderId1",
                table: "OrderStatusHistories",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderStatusHistories_OrderId1",
                table: "OrderStatusHistories",
                column: "OrderId1");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderStatusHistories_Orders_OrderId1",
                table: "OrderStatusHistories",
                column: "OrderId1",
                principalTable: "Orders",
                principalColumn: "Id");
        }
    }
}
