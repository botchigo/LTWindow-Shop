using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Database.Migrations
{
    /// <inheritdoc />
    public partial class SeedDataForProductAndCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "category",
                columns: new[] { "category_id", "created_at", "Description", "Name", "updated_at" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 12, 16, 1, 47, 35, 779, DateTimeKind.Utc).AddTicks(9087), "Laptop các loại", "Laptop", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Phụ kiện máy tính", "Phụ kiện", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) }
                });

            migrationBuilder.InsertData(
                table: "product",
                columns: new[] { "product_id", "category_id", "Count", "created_at", "Description", "import_price", "Name", "sale_price", "Sku", "updated_at" },
                values: new object[,]
                {
                    { 1, 1, 10, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "MacBook Pro chip M3", 25000000m, "MacBook Pro M3", 30000000m, "LAP001", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 2, 2, 50, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Chuột không dây Logitech", 300000m, "Chuột Logitech", 450000m, "ACC001", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_product_Name",
                table: "product",
                column: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_product_Name",
                table: "product");

            migrationBuilder.DeleteData(
                table: "product",
                keyColumn: "product_id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "product",
                keyColumn: "product_id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "category",
                keyColumn: "category_id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "category",
                keyColumn: "category_id",
                keyValue: 2);
        }
    }
}
