using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Database.Migrations
{
    /// <inheritdoc />
    public partial class SeedOrderData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "category",
                keyColumn: "category_id",
                keyValue: 1,
                column: "created_at",
                value: new DateTime(2025, 12, 17, 19, 0, 30, 538, DateTimeKind.Utc).AddTicks(7270));

            migrationBuilder.InsertData(
                table: "order",
                columns: new[] { "order_id", "created_at", "final_price", "payment_method", "status", "updated_at", "user_id" },
                values: new object[,]
                {
                    { 101, new DateTime(2025, 12, 3, 10, 0, 0, 0, DateTimeKind.Utc), 500000m, 2, 5, new DateTime(2025, 12, 3, 10, 0, 0, 0, DateTimeKind.Utc), 1 },
                    { 102, new DateTime(2025, 12, 6, 10, 0, 0, 0, DateTimeKind.Utc), 1000000m, 2, 5, new DateTime(2025, 12, 6, 10, 0, 0, 0, DateTimeKind.Utc), 1 },
                    { 103, new DateTime(2025, 12, 9, 10, 0, 0, 0, DateTimeKind.Utc), 1500000m, 2, 5, new DateTime(2025, 12, 9, 10, 0, 0, 0, DateTimeKind.Utc), 1 },
                    { 104, new DateTime(2025, 12, 12, 10, 0, 0, 0, DateTimeKind.Utc), 2000000m, 2, 5, new DateTime(2025, 12, 12, 10, 0, 0, 0, DateTimeKind.Utc), 1 },
                    { 105, new DateTime(2025, 12, 15, 10, 0, 0, 0, DateTimeKind.Utc), 2500000m, 2, 5, new DateTime(2025, 12, 15, 10, 0, 0, 0, DateTimeKind.Utc), 1 },
                    { 106, new DateTime(2025, 12, 18, 10, 0, 0, 0, DateTimeKind.Utc), 3000000m, 2, 5, new DateTime(2025, 12, 18, 10, 0, 0, 0, DateTimeKind.Utc), 1 },
                    { 107, new DateTime(2025, 12, 21, 10, 0, 0, 0, DateTimeKind.Utc), 3500000m, 2, 5, new DateTime(2025, 12, 21, 10, 0, 0, 0, DateTimeKind.Utc), 1 }
                });

            migrationBuilder.InsertData(
                table: "order_item",
                columns: new[] { "order_item_id", "order_id", "product_id", "quantity", "total_price", "unit_cost", "unit_sale_price" },
                values: new object[,]
                {
                    { 101, 101, 2, 1, 500000m, 350000.0m, 500000m },
                    { 102, 102, 1, 2, 1000000m, 350000.0m, 500000m },
                    { 103, 103, 2, 3, 1500000m, 350000.0m, 500000m },
                    { 104, 104, 1, 4, 2000000m, 350000.0m, 500000m },
                    { 105, 105, 2, 5, 2500000m, 350000.0m, 500000m },
                    { 106, 106, 1, 6, 3000000m, 350000.0m, 500000m },
                    { 107, 107, 2, 7, 3500000m, 350000.0m, 500000m }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "order_item",
                keyColumn: "order_item_id",
                keyValue: 101);

            migrationBuilder.DeleteData(
                table: "order_item",
                keyColumn: "order_item_id",
                keyValue: 102);

            migrationBuilder.DeleteData(
                table: "order_item",
                keyColumn: "order_item_id",
                keyValue: 103);

            migrationBuilder.DeleteData(
                table: "order_item",
                keyColumn: "order_item_id",
                keyValue: 104);

            migrationBuilder.DeleteData(
                table: "order_item",
                keyColumn: "order_item_id",
                keyValue: 105);

            migrationBuilder.DeleteData(
                table: "order_item",
                keyColumn: "order_item_id",
                keyValue: 106);

            migrationBuilder.DeleteData(
                table: "order_item",
                keyColumn: "order_item_id",
                keyValue: 107);

            migrationBuilder.DeleteData(
                table: "order",
                keyColumn: "order_id",
                keyValue: 101);

            migrationBuilder.DeleteData(
                table: "order",
                keyColumn: "order_id",
                keyValue: 102);

            migrationBuilder.DeleteData(
                table: "order",
                keyColumn: "order_id",
                keyValue: 103);

            migrationBuilder.DeleteData(
                table: "order",
                keyColumn: "order_id",
                keyValue: 104);

            migrationBuilder.DeleteData(
                table: "order",
                keyColumn: "order_id",
                keyValue: 105);

            migrationBuilder.DeleteData(
                table: "order",
                keyColumn: "order_id",
                keyValue: 106);

            migrationBuilder.DeleteData(
                table: "order",
                keyColumn: "order_id",
                keyValue: 107);

            migrationBuilder.UpdateData(
                table: "category",
                keyColumn: "category_id",
                keyValue: 1,
                column: "created_at",
                value: new DateTime(2025, 12, 17, 13, 36, 47, 568, DateTimeKind.Utc).AddTicks(3858));
        }
    }
}
