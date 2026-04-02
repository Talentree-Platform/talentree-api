using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Talentree.Repository.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAIExtraColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "MaterialOrders");

            migrationBuilder.AddColumn<int>(
                name: "OrderFrequency",
                table: "RawMaterials",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "PriceTrend",
                table: "RawMaterials",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PaymentStatus",
                table: "MaterialOrders",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Unpaid");

            migrationBuilder.AddColumn<byte>(
                name: "ProfileCompletenessPct",
                table: "BusinessOwnerProfile",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<float>(
                name: "FraudScore",
                table: "BoProductionRequests",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FulfillmentTimeHours",
                table: "BoProductionRequests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFraudFlag",
                table: "BoProductionRequests",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PaymentStatus",
                table: "BoProductionRequests",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Unpaid");

            migrationBuilder.AddColumn<string>(
                name: "StripePaymentIntentId",
                table: "BoProductionRequests",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "ChurnRiskScore",
                table: "AspNetUsers",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ChurnRiskUpdatedAt",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LoginCount",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "PayoutRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BusinessOwnerId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValue: "EGP"),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BankName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    AccountHolderName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    AccountIdentifierEnc = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RoutingSwiftCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RejectionReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ProcessedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayoutRequests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BusinessOwnerId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    BalanceAfter = table.Column<decimal>(type: "decimal(14,2)", nullable: false),
                    ReferenceId = table.Column<int>(type: "int", nullable: true),
                    ReferenceType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    StripePaymentIntentId = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    AnomalyFlag = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    AnomalyScore = table.Column<float>(type: "real", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PayoutRequests_BusinessOwnerId_Status",
                table: "PayoutRequests",
                columns: new[] { "BusinessOwnerId", "Status" },
                unique: true,
                filter: "[Status] = 'Pending'");

            migrationBuilder.CreateIndex(
                name: "IX_PayoutRequests_Status",
                table: "PayoutRequests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_BusinessOwnerId_CreatedAt",
                table: "Transactions",
                columns: new[] { "BusinessOwnerId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_StripePaymentIntentId",
                table: "Transactions",
                column: "StripePaymentIntentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PayoutRequests");

            migrationBuilder.DropTable(
                name: "Transactions");

            migrationBuilder.DropColumn(
                name: "OrderFrequency",
                table: "RawMaterials");

            migrationBuilder.DropColumn(
                name: "PriceTrend",
                table: "RawMaterials");

            migrationBuilder.DropColumn(
                name: "PaymentStatus",
                table: "MaterialOrders");

            migrationBuilder.DropColumn(
                name: "ProfileCompletenessPct",
                table: "BusinessOwnerProfile");

            migrationBuilder.DropColumn(
                name: "FraudScore",
                table: "BoProductionRequests");

            migrationBuilder.DropColumn(
                name: "FulfillmentTimeHours",
                table: "BoProductionRequests");

            migrationBuilder.DropColumn(
                name: "IsFraudFlag",
                table: "BoProductionRequests");

            migrationBuilder.DropColumn(
                name: "PaymentStatus",
                table: "BoProductionRequests");

            migrationBuilder.DropColumn(
                name: "StripePaymentIntentId",
                table: "BoProductionRequests");

            migrationBuilder.DropColumn(
                name: "ChurnRiskScore",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ChurnRiskUpdatedAt",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "LoginCount",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<string>(
                name: "PaymentMethod",
                table: "MaterialOrders",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }
    }
}
