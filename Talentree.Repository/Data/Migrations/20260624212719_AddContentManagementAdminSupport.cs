using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Talentree.Repository.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddContentManagementAdminSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "KnowledgeArticles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "KnowledgeArticles",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "KnowledgeArticles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "TotalViewDurationSeconds",
                table: "KnowledgeArticles",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "ContentSearchLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SearchTerm = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    SearchedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentSearchLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContentSearchLogs_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_KnowledgeArticles_IsDeleted",
                table: "KnowledgeArticles",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_ContentSearchLogs_SearchedAt",
                table: "ContentSearchLogs",
                column: "SearchedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ContentSearchLogs_SearchTerm",
                table: "ContentSearchLogs",
                column: "SearchTerm");

            migrationBuilder.CreateIndex(
                name: "IX_ContentSearchLogs_UserId",
                table: "ContentSearchLogs",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContentSearchLogs");

            migrationBuilder.DropIndex(
                name: "IX_KnowledgeArticles_IsDeleted",
                table: "KnowledgeArticles");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "KnowledgeArticles");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "KnowledgeArticles");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "KnowledgeArticles");

            migrationBuilder.DropColumn(
                name: "TotalViewDurationSeconds",
                table: "KnowledgeArticles");
        }
    }
}
