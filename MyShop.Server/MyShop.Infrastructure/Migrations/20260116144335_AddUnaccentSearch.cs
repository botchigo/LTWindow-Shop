using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NpgsqlTypes;

#nullable disable

namespace MyShop.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUnaccentSearch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //Create unaccent extension
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:unaccent", ",,");

            // B. Tạo cấu hình tìm kiếm tùy chỉnh tên là "vn_unaccent"
            // Logic: Copy từ 'simple' -> Áp dụng bộ lọc 'unaccent' -> Áp dụng 'simple'
            migrationBuilder.Sql(@"
            CREATE TEXT SEARCH CONFIGURATION vn_unaccent (COPY = simple);
            ALTER TEXT SEARCH CONFIGURATION vn_unaccent
                ALTER MAPPING FOR hword, hword_part, word
                WITH unaccent, simple;
            ");

            migrationBuilder.AddColumn<NpgsqlTsVector>(
                name: "SearchVector",
                table: "product",
                type: "tsvector",
                nullable: false)
                .Annotation("Npgsql:TsVectorConfig", "vn_unaccent")
                .Annotation("Npgsql:TsVectorProperties", new[] { "name", "Description" });

            migrationBuilder.UpdateData(
                table: "category",
                keyColumn: "category_id",
                keyValue: 1,
                column: "created_at",
                value: new DateTime(2026, 1, 16, 14, 43, 32, 788, DateTimeKind.Utc).AddTicks(4649));

            migrationBuilder.CreateIndex(
                name: "IX_product_SearchVector",
                table: "product",
                column: "SearchVector")
                .Annotation("Npgsql:IndexMethod", "GIN");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_product_SearchVector",
                table: "product");

            migrationBuilder.DropColumn(
                name: "SearchVector",
                table: "product");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:PostgresExtension:unaccent", ",,");

            migrationBuilder.UpdateData(
                table: "category",
                keyColumn: "category_id",
                keyValue: 1,
                column: "created_at",
                value: new DateTime(2025, 12, 25, 2, 48, 6, 552, DateTimeKind.Utc).AddTicks(8270));
        }
    }
}
