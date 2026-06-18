using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Talentree.Repository.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRfmSegmentToAppUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MaterialOrderItems_RawMaterials_RawMaterialId1",
                table: "MaterialOrderItems");

            migrationBuilder.DropIndex(
                name: "IX_MaterialOrderItems_RawMaterialId1",
                table: "MaterialOrderItems");

            migrationBuilder.DropColumn(
                name: "RawMaterialId1",
                table: "MaterialOrderItems");

            migrationBuilder.AddColumn<string>(
                name: "RfmSegment",
                table: "AspNetUsers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ProcessedMessages",
                columns: table => new
                {
                    MessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessedMessages", x => x.MessageId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProcessedMessages");

            migrationBuilder.DropColumn(
                name: "RfmSegment",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<int>(
                name: "RawMaterialId1",
                table: "MaterialOrderItems",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MaterialOrderItems_RawMaterialId1",
                table: "MaterialOrderItems",
                column: "RawMaterialId1");

            migrationBuilder.AddForeignKey(
                name: "FK_MaterialOrderItems_RawMaterials_RawMaterialId1",
                table: "MaterialOrderItems",
                column: "RawMaterialId1",
                principalTable: "RawMaterials",
                principalColumn: "Id");
        }
    }
}
