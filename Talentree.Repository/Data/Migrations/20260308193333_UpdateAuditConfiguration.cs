using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Talentree.Repository.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAuditConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RefreshTokens_IsDeleted",
                table: "RefreshTokens");

            migrationBuilder.DropIndex(
                name: "IX_ProductImages_IsDeleted",
                table: "ProductImages");

            migrationBuilder.DropIndex(
                name: "IX_Address_IsDeleted",
                table: "Address");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "ProductImages");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "ProductImages");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "ProductImages");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "ProductImages");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "ProductImages");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "ProductImages");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "MaterialBaskets");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "MaterialBaskets");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "MaterialBasketItems");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "MaterialBasketItems");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "MaterialBasketItems");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "MaterialBasketItems");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "MaterialBasketItems");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "MaterialBasketItems");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Address");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Address");

            migrationBuilder.RenameIndex(
                name: "IX_Products_IsDeleted",
                table: "Products",
                newName: "IX_Product_IsDeleted");

            migrationBuilder.RenameIndex(
                name: "IX_Categories_IsDeleted",
                table: "Categories",
                newName: "IX_Category_IsDeleted");

            migrationBuilder.AlterColumn<string>(
                name: "UpdatedBy",
                table: "Suppliers",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "Suppliers",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Suppliers",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "Suppliers",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UpdatedBy",
                table: "RawMaterials",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "RawMaterials",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "RawMaterials",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "RawMaterials",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "RawMaterials",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UpdatedBy",
                table: "Products",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "Products",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "Products",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UpdatedBy",
                table: "MaterialBaskets",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "MaterialBaskets",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "MaterialBaskets",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "UpdatedBy",
                table: "Categories",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "Categories",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "Categories",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UpdatedBy",
                table: "BusinessOwnerProfile",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "BusinessOwnerProfile",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "BusinessOwnerProfile",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UpdatedBy",
                table: "Address",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "Address",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Supplier_CreatedAt",
                table: "Suppliers",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Supplier_CreatedBy_CreatedAt",
                table: "Suppliers",
                columns: new[] { "CreatedBy", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Supplier_IsDeleted",
                table: "Suppliers",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Supplier_IsDeleted_DeletedAt",
                table: "Suppliers",
                columns: new[] { "IsDeleted", "DeletedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_RawMaterial_CreatedAt",
                table: "RawMaterials",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_RawMaterial_CreatedBy_CreatedAt",
                table: "RawMaterials",
                columns: new[] { "CreatedBy", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_RawMaterial_IsDeleted",
                table: "RawMaterials",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_RawMaterial_IsDeleted_DeletedAt",
                table: "RawMaterials",
                columns: new[] { "IsDeleted", "DeletedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Product_CreatedAt",
                table: "Products",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Product_CreatedBy_CreatedAt",
                table: "Products",
                columns: new[] { "CreatedBy", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Product_IsDeleted_DeletedAt",
                table: "Products",
                columns: new[] { "IsDeleted", "DeletedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_MaterialBasket_CreatedAt",
                table: "MaterialBaskets",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialBasket_CreatedBy_CreatedAt",
                table: "MaterialBaskets",
                columns: new[] { "CreatedBy", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Category_CreatedAt",
                table: "Categories",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Category_CreatedBy_CreatedAt",
                table: "Categories",
                columns: new[] { "CreatedBy", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Category_IsDeleted_DeletedAt",
                table: "Categories",
                columns: new[] { "IsDeleted", "DeletedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_BusinessOwnerProfile_CreatedAt",
                table: "BusinessOwnerProfile",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessOwnerProfile_CreatedBy_CreatedAt",
                table: "BusinessOwnerProfile",
                columns: new[] { "CreatedBy", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_BusinessOwnerProfile_IsDeleted_DeletedAt",
                table: "BusinessOwnerProfile",
                columns: new[] { "IsDeleted", "DeletedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Address_CreatedAt",
                table: "Address",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Address_CreatedBy_CreatedAt",
                table: "Address",
                columns: new[] { "CreatedBy", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Supplier_CreatedAt",
                table: "Suppliers");

            migrationBuilder.DropIndex(
                name: "IX_Supplier_CreatedBy_CreatedAt",
                table: "Suppliers");

            migrationBuilder.DropIndex(
                name: "IX_Supplier_IsDeleted",
                table: "Suppliers");

            migrationBuilder.DropIndex(
                name: "IX_Supplier_IsDeleted_DeletedAt",
                table: "Suppliers");

            migrationBuilder.DropIndex(
                name: "IX_RawMaterial_CreatedAt",
                table: "RawMaterials");

            migrationBuilder.DropIndex(
                name: "IX_RawMaterial_CreatedBy_CreatedAt",
                table: "RawMaterials");

            migrationBuilder.DropIndex(
                name: "IX_RawMaterial_IsDeleted",
                table: "RawMaterials");

            migrationBuilder.DropIndex(
                name: "IX_RawMaterial_IsDeleted_DeletedAt",
                table: "RawMaterials");

            migrationBuilder.DropIndex(
                name: "IX_Product_CreatedAt",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Product_CreatedBy_CreatedAt",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Product_IsDeleted_DeletedAt",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_MaterialBasket_CreatedAt",
                table: "MaterialBaskets");

            migrationBuilder.DropIndex(
                name: "IX_MaterialBasket_CreatedBy_CreatedAt",
                table: "MaterialBaskets");

            migrationBuilder.DropIndex(
                name: "IX_Category_CreatedAt",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Category_CreatedBy_CreatedAt",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Category_IsDeleted_DeletedAt",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_BusinessOwnerProfile_CreatedAt",
                table: "BusinessOwnerProfile");

            migrationBuilder.DropIndex(
                name: "IX_BusinessOwnerProfile_CreatedBy_CreatedAt",
                table: "BusinessOwnerProfile");

            migrationBuilder.DropIndex(
                name: "IX_BusinessOwnerProfile_IsDeleted_DeletedAt",
                table: "BusinessOwnerProfile");

            migrationBuilder.DropIndex(
                name: "IX_Address_CreatedAt",
                table: "Address");

            migrationBuilder.DropIndex(
                name: "IX_Address_CreatedBy_CreatedAt",
                table: "Address");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "RawMaterials");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "BusinessOwnerProfile");

            migrationBuilder.RenameIndex(
                name: "IX_Product_IsDeleted",
                table: "Products",
                newName: "IX_Products_IsDeleted");

            migrationBuilder.RenameIndex(
                name: "IX_Category_IsDeleted",
                table: "Categories",
                newName: "IX_Categories_IsDeleted");

            migrationBuilder.AlterColumn<string>(
                name: "UpdatedBy",
                table: "Suppliers",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450,
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "Suppliers",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Suppliers",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "RefreshTokens",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "RefreshTokens",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "RefreshTokens",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "RefreshTokens",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "RefreshTokens",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UpdatedBy",
                table: "RawMaterials",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450,
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "RawMaterials",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "RawMaterials",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "RawMaterials",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<string>(
                name: "UpdatedBy",
                table: "Products",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "Products",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450,
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "ProductImages",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "ProductImages",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "ProductImages",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "ProductImages",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "ProductImages",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "ProductImages",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UpdatedBy",
                table: "MaterialBaskets",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "MaterialBaskets",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "MaterialBaskets",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "MaterialBaskets",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "MaterialBaskets",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "MaterialBasketItems",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "MaterialBasketItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "MaterialBasketItems",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "MaterialBasketItems",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "MaterialBasketItems",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "MaterialBasketItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UpdatedBy",
                table: "Categories",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "Categories",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UpdatedBy",
                table: "BusinessOwnerProfile",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "BusinessOwnerProfile",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UpdatedBy",
                table: "Address",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "Address",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450,
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Address",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Address",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_IsDeleted",
                table: "RefreshTokens",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_ProductImages_IsDeleted",
                table: "ProductImages",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Address_IsDeleted",
                table: "Address",
                column: "IsDeleted");
        }
    }
}
