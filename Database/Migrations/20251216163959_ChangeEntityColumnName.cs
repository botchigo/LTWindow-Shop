using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Database.Migrations
{
    /// <inheritdoc />
    public partial class ChangeEntityColumnName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Sku",
                table: "product",
                newName: "sku");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "product",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Count",
                table: "product",
                newName: "count");

            migrationBuilder.RenameIndex(
                name: "IX_product_Name",
                table: "product",
                newName: "IX_product_name");

            migrationBuilder.RenameColumn(
                name: "Quantity",
                table: "order_item",
                newName: "quantity");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "order",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "category",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "category",
                newName: "description");

            migrationBuilder.AlterColumn<string>(
                name: "sku",
                table: "product",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "product",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "product",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "category",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "category",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "category",
                keyColumn: "category_id",
                keyValue: 1,
                column: "created_at",
                value: new DateTime(2025, 12, 16, 16, 39, 56, 571, DateTimeKind.Utc).AddTicks(1753));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "sku",
                table: "product",
                newName: "Sku");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "product",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "count",
                table: "product",
                newName: "Count");

            migrationBuilder.RenameIndex(
                name: "IX_product_name",
                table: "product",
                newName: "IX_product_Name");

            migrationBuilder.RenameColumn(
                name: "quantity",
                table: "order_item",
                newName: "Quantity");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "order",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "category",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "category",
                newName: "Description");

            migrationBuilder.AlterColumn<string>(
                name: "Sku",
                table: "product",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "product",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "product",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "category",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "category",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.UpdateData(
                table: "category",
                keyColumn: "category_id",
                keyValue: 1,
                column: "created_at",
                value: new DateTime(2025, 12, 16, 1, 47, 35, 779, DateTimeKind.Utc).AddTicks(9087));
        }
    }
}
