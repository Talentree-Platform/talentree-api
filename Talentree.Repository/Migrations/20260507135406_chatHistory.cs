using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Talentree.Repository.Migrations
{
    /// <inheritdoc />
    public partial class chatHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BrandTone",
                table: "BusinessOwnerProfile",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TargetAudience",
                table: "BusinessOwnerProfile",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ChatHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BusinessOwnerProfileId = table.Column<int>(type: "int", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Agent = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatHistory_BusinessOwnerProfile_BusinessOwnerProfileId",
                        column: x => x.BusinessOwnerProfileId,
                        principalTable: "BusinessOwnerProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChatHistory_Agent",
                table: "ChatHistory",
                column: "Agent");

            migrationBuilder.CreateIndex(
                name: "IX_ChatHistory_BusinessOwnerProfileId",
                table: "ChatHistory",
                column: "BusinessOwnerProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatHistory_BusinessOwnerProfileId_CreatedAt",
                table: "ChatHistory",
                columns: new[] { "BusinessOwnerProfileId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ChatHistory_CreatedAt",
                table: "ChatHistory",
                column: "CreatedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChatHistory");

            migrationBuilder.DropColumn(
                name: "BrandTone",
                table: "BusinessOwnerProfile");

            migrationBuilder.DropColumn(
                name: "TargetAudience",
                table: "BusinessOwnerProfile");
        }
    }
}
