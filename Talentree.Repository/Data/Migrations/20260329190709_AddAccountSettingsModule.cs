using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Talentree.Repository.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountSettingsModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BusinessLogoUrl",
                table: "BusinessOwnerProfile",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "BusinessOwnerProfile",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProfilePhotoUrl",
                table: "BusinessOwnerProfile",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BusinessOwnerPaymentInfos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    BankName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AccountHolderName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AccountNumberEncrypted = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    RoutingSwiftCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastChangedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastChangedByIp = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessOwnerPaymentInfos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BusinessOwnerPaymentInfos_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LoginHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DeviceInfo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Location = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    LoginAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsSuccessful = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoginHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoginHistories_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserPreferences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Timezone = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, defaultValue: "Africa/Cairo"),
                    DateFormat = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "dd/MM/yyyy"),
                    CurrencyDisplay = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValue: "EGP"),
                    DashboardLayout = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "default"),
                    Language = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPreferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserPreferences_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BusinessOwnerPaymentInfos_UserId",
                table: "BusinessOwnerPaymentInfos",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LoginHistories_LoginAt",
                table: "LoginHistories",
                column: "LoginAt");

            migrationBuilder.CreateIndex(
                name: "IX_LoginHistories_UserId",
                table: "LoginHistories",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPreferences_UserId",
                table: "UserPreferences",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BusinessOwnerPaymentInfos");

            migrationBuilder.DropTable(
                name: "LoginHistories");

            migrationBuilder.DropTable(
                name: "UserPreferences");

            migrationBuilder.DropColumn(
                name: "BusinessLogoUrl",
                table: "BusinessOwnerProfile");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "BusinessOwnerProfile");

            migrationBuilder.DropColumn(
                name: "ProfilePhotoUrl",
                table: "BusinessOwnerProfile");
        }
    }
}
