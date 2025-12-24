using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MyShop.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initital : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "app_user",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    username = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    password = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    full_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_app_user", x => x.user_id);
                });

            migrationBuilder.CreateTable(
                name: "category",
                columns: table => new
                {
                    category_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_category", x => x.category_id);
                });

            migrationBuilder.CreateTable(
                name: "order",
                columns: table => new
                {
                    order_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    final_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    payment_method = table.Column<int>(type: "integer", nullable: false),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order", x => x.order_id);
                    table.ForeignKey(
                        name: "FK_order_app_user_user_id",
                        column: x => x.user_id,
                        principalTable: "app_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "product",
                columns: table => new
                {
                    product_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    sku = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    import_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    sale_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    sale_amount = table.Column<int>(type: "integer", nullable: false),
                    stock = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    category_id = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product", x => x.product_id);
                    table.ForeignKey(
                        name: "FK_product_category_category_id",
                        column: x => x.category_id,
                        principalTable: "category",
                        principalColumn: "category_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "order_item",
                columns: table => new
                {
                    order_item_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    unit_sale_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    unit_cost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    total_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    order_id = table.Column<int>(type: "integer", nullable: false),
                    product_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_item", x => x.order_item_id);
                    table.ForeignKey(
                        name: "FK_order_item_order_order_id",
                        column: x => x.order_id,
                        principalTable: "order",
                        principalColumn: "order_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_order_item_product_product_id",
                        column: x => x.product_id,
                        principalTable: "product",
                        principalColumn: "product_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "product_image",
                columns: table => new
                {
                    image_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    image_path = table.Column<string>(type: "text", nullable: false),
                    product_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_image", x => x.image_id);
                    table.ForeignKey(
                        name: "FK_product_image_product_product_id",
                        column: x => x.product_id,
                        principalTable: "product",
                        principalColumn: "product_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "app_user",
                columns: new[] { "user_id", "full_name", "password", "username" },
                values: new object[] { 1, "admin", "123", "admin" });

            migrationBuilder.InsertData(
                table: "category",
                columns: new[] { "category_id", "created_at", "description", "name", "updated_at" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 12, 24, 14, 41, 8, 247, DateTimeKind.Utc).AddTicks(5717), "Laptop các loại", "Laptop", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Phụ kiện máy tính", "Phụ kiện", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) }
                });

            migrationBuilder.InsertData(
                table: "order",
                columns: new[] { "order_id", "created_at", "final_price", "payment_method", "status", "updated_at", "user_id" },
                values: new object[,]
                {
                    { 101, new DateTime(2025, 12, 3, 10, 0, 0, 0, DateTimeKind.Utc), 500000m, 2, 1, new DateTime(2025, 12, 3, 10, 0, 0, 0, DateTimeKind.Utc), 1 },
                    { 102, new DateTime(2025, 12, 6, 10, 0, 0, 0, DateTimeKind.Utc), 1000000m, 2, 1, new DateTime(2025, 12, 6, 10, 0, 0, 0, DateTimeKind.Utc), 1 },
                    { 103, new DateTime(2025, 12, 9, 10, 0, 0, 0, DateTimeKind.Utc), 1500000m, 2, 1, new DateTime(2025, 12, 9, 10, 0, 0, 0, DateTimeKind.Utc), 1 },
                    { 104, new DateTime(2025, 12, 12, 10, 0, 0, 0, DateTimeKind.Utc), 2000000m, 2, 1, new DateTime(2025, 12, 12, 10, 0, 0, 0, DateTimeKind.Utc), 1 },
                    { 105, new DateTime(2025, 12, 15, 10, 0, 0, 0, DateTimeKind.Utc), 2500000m, 2, 1, new DateTime(2025, 12, 15, 10, 0, 0, 0, DateTimeKind.Utc), 1 },
                    { 106, new DateTime(2025, 12, 18, 10, 0, 0, 0, DateTimeKind.Utc), 3000000m, 2, 1, new DateTime(2025, 12, 18, 10, 0, 0, 0, DateTimeKind.Utc), 1 },
                    { 107, new DateTime(2025, 12, 21, 10, 0, 0, 0, DateTimeKind.Utc), 3500000m, 2, 1, new DateTime(2025, 12, 21, 10, 0, 0, 0, DateTimeKind.Utc), 1 }
                });

            migrationBuilder.InsertData(
                table: "product",
                columns: new[] { "product_id", "category_id", "created_at", "Description", "import_price", "name", "sale_amount", "sale_price", "sku", "stock", "updated_at" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "MacBook Pro chip M3", 25000000m, "MacBook Pro M3", 0, 30000000m, "LAP001", 10, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 2, 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Chuột không dây Logitech", 300000m, "Chuột Logitech", 30, 450000m, "ACC001", 50, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) }
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

            migrationBuilder.CreateIndex(
                name: "IX_app_user_username",
                table: "app_user",
                column: "username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_order_user_id",
                table: "order",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_item_order_id",
                table: "order_item",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_item_product_id",
                table: "order_item",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_product_category_id",
                table: "product",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_product_name",
                table: "product",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "IX_product_image_product_id",
                table: "product_image",
                column: "product_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "order_item");

            migrationBuilder.DropTable(
                name: "product_image");

            migrationBuilder.DropTable(
                name: "order");

            migrationBuilder.DropTable(
                name: "product");

            migrationBuilder.DropTable(
                name: "app_user");

            migrationBuilder.DropTable(
                name: "category");
        }
    }
}
