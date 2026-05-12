using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Talentree.Repository.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMaterialOrdersAndProductionRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BoProductionRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BusinessOwnerId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    QuotedPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    AdminNotes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    EstimatedCompletionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoProductionRequests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BoProductionRequestItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BoProductionRequestId = table.Column<int>(type: "int", nullable: false),
                    ProductType = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    PreferredRawMaterialId = table.Column<int>(type: "int", nullable: true),
                    Specifications = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoProductionRequestItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BoProductionRequestItems_BoProductionRequests_BoProductionRequestId",
                        column: x => x.BoProductionRequestId,
                        principalTable: "BoProductionRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BoProductionRequestItems_RawMaterials_PreferredRawMaterialId",
                        column: x => x.PreferredRawMaterialId,
                        principalTable: "RawMaterials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BoProductionRequestStatusHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BoProductionRequestId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ChangedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoProductionRequestStatusHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BoProductionRequestStatusHistories_BoProductionRequests_BoProductionRequestId",
                        column: x => x.BoProductionRequestId,
                        principalTable: "BoProductionRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BoProductionRequestItems_BoProductionRequestId",
                table: "BoProductionRequestItems",
                column: "BoProductionRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_BoProductionRequestItems_PreferredRawMaterialId",
                table: "BoProductionRequestItems",
                column: "PreferredRawMaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_BoProductionRequests_BusinessOwnerId",
                table: "BoProductionRequests",
                column: "BusinessOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_BoProductionRequests_Status",
                table: "BoProductionRequests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_BoProductionRequestStatusHistories_BoProductionRequestId",
                table: "BoProductionRequestStatusHistories",
                column: "BoProductionRequestId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BoProductionRequestItems");

            migrationBuilder.DropTable(
                name: "BoProductionRequestStatusHistories");

            migrationBuilder.DropTable(
                name: "BoProductionRequests");
        }
    }
}
