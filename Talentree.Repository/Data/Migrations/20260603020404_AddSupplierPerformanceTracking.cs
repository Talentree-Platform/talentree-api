using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Talentree.Repository.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSupplierPerformanceTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RelatedSupplierId",
                table: "SupportTickets",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeliveredAt",
                table: "MaterialOrders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SupplierReviews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SupplierId = table.Column<int>(type: "int", nullable: false),
                    BusinessOwnerUserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BusinessOwnerId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    MaterialOrderId = table.Column<int>(type: "int", nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierReviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupplierReviews_AspNetUsers_BusinessOwnerId",
                        column: x => x.BusinessOwnerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SupplierReviews_MaterialOrders_MaterialOrderId",
                        column: x => x.MaterialOrderId,
                        principalTable: "MaterialOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SupplierReviews_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SupportTickets_RelatedSupplierId",
                table: "SupportTickets",
                column: "RelatedSupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierReviews_BusinessOwnerId",
                table: "SupplierReviews",
                column: "BusinessOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierReviews_MaterialOrderId",
                table: "SupplierReviews",
                column: "MaterialOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierReviews_SupplierId",
                table: "SupplierReviews",
                column: "SupplierId");

            migrationBuilder.AddForeignKey(
                name: "FK_SupportTickets_Suppliers_RelatedSupplierId",
                table: "SupportTickets",
                column: "RelatedSupplierId",
                principalTable: "Suppliers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SupportTickets_Suppliers_RelatedSupplierId",
                table: "SupportTickets");

            migrationBuilder.DropTable(
                name: "SupplierReviews");

            migrationBuilder.DropIndex(
                name: "IX_SupportTickets_RelatedSupplierId",
                table: "SupportTickets");

            migrationBuilder.DropColumn(
                name: "RelatedSupplierId",
                table: "SupportTickets");

            migrationBuilder.DropColumn(
                name: "DeliveredAt",
                table: "MaterialOrders");
        }
    }
}
