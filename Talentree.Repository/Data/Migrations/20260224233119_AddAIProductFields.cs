using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Talentree.Repository.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAIProductFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "ProductImages");

            migrationBuilder.AddColumn<float>(
                name: "AvgRating",
                table: "Products",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "CartAddCount",
                table: "Products",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<int>(
                name: "DemandForecastQty",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DemandForecastUpdatedAt",
                table: "Products",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "DescriptionQualityScore",
                table: "Products",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "LowStockFlag",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "PurchaseCount",
                table: "Products",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<decimal>(
                name: "RevenueTotal",
                table: "Products",
                type: "decimal(14,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<long>(
                name: "ViewCount",
                table: "Products",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                table: "ProductImages",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "BusinessType",
                table: "Categories",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvgRating",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CartAddCount",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "DemandForecastQty",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "DemandForecastUpdatedAt",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "DescriptionQualityScore",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "LowStockFlag",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "PurchaseCount",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "RevenueTotal",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ViewCount",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                table: "ProductImages");

            migrationBuilder.DropColumn(
                name: "BusinessType",
                table: "Categories");

            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "ProductImages",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
