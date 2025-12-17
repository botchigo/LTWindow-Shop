using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Database.Migrations
{
    /// <inheritdoc />
    public partial class AddStockAndSaleAmountForProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "count",
                table: "product",
                newName: "stock");

            migrationBuilder.AddColumn<int>(
                name: "sale_amount",
                table: "product",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "category",
                keyColumn: "category_id",
                keyValue: 1,
                column: "created_at",
                value: new DateTime(2025, 12, 17, 13, 36, 47, 568, DateTimeKind.Utc).AddTicks(3858));

            migrationBuilder.UpdateData(
                table: "product",
                keyColumn: "product_id",
                keyValue: 1,
                column: "sale_amount",
                value: 0);

            migrationBuilder.UpdateData(
                table: "product",
                keyColumn: "product_id",
                keyValue: 2,
                column: "sale_amount",
                value: 30);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "sale_amount",
                table: "product");

            migrationBuilder.RenameColumn(
                name: "stock",
                table: "product",
                newName: "count");

            migrationBuilder.UpdateData(
                table: "category",
                keyColumn: "category_id",
                keyValue: 1,
                column: "created_at",
                value: new DateTime(2025, 12, 16, 18, 11, 24, 231, DateTimeKind.Utc).AddTicks(2704));
        }
    }
}
