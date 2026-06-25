using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Talentree.Repository.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSecuritySettingsAndEnhanceUserActionLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "UserActionLogs",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450);

            migrationBuilder.AlterColumn<string>(
                name: "AdminId",
                table: "UserActionLogs",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450);

            migrationBuilder.AddColumn<string>(
                name: "AfterValues",
                table: "UserActionLogs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BeforeValues",
                table: "UserActionLogs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EntityId",
                table: "UserActionLogs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EntityType",
                table: "UserActionLogs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IpAddress",
                table: "UserActionLogs",
                type: "nvarchar(45)",
                maxLength: 45,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SecuritySettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PasswordRequiredLength = table.Column<int>(type: "int", nullable: false),
                    PasswordRequireDigit = table.Column<bool>(type: "bit", nullable: false),
                    PasswordRequireLowercase = table.Column<bool>(type: "bit", nullable: false),
                    PasswordRequireUppercase = table.Column<bool>(type: "bit", nullable: false),
                    PasswordRequireNonAlphanumeric = table.Column<bool>(type: "bit", nullable: false),
                    SessionTimeoutInMinutes = table.Column<int>(type: "int", nullable: false),
                    MaxFailedAccessAttempts = table.Column<int>(type: "int", nullable: false),
                    LockoutDurationInMinutes = table.Column<int>(type: "int", nullable: false),
                    RequireTwoFactorForAdmins = table.Column<bool>(type: "bit", nullable: false),
                    IpWhitelist = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    AllowedLoginStartTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    AllowedLoginEndTime = table.Column<TimeSpan>(type: "time", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SecuritySettings", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SecuritySettings");

            migrationBuilder.DropColumn(
                name: "AfterValues",
                table: "UserActionLogs");

            migrationBuilder.DropColumn(
                name: "BeforeValues",
                table: "UserActionLogs");

            migrationBuilder.DropColumn(
                name: "EntityId",
                table: "UserActionLogs");

            migrationBuilder.DropColumn(
                name: "EntityType",
                table: "UserActionLogs");

            migrationBuilder.DropColumn(
                name: "IpAddress",
                table: "UserActionLogs");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "UserActionLogs",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AdminId",
                table: "UserActionLogs",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450,
                oldNullable: true);
        }
    }
}
