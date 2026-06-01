using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Talentree.Repository.Data.Migrations
{
    /// <inheritdoc />
    public partial class updateNotificationSys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Notification_CreatedBy_CreatedAt",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_ExpiresAt",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_RelatedEntity",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_UserId_IsRead_CreatedAt",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_UserId_Type",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_NotificationPreference_CreatedAt",
                table: "NotificationPreferences");

            migrationBuilder.DropIndex(
                name: "IX_NotificationPreference_CreatedBy_CreatedAt",
                table: "NotificationPreferences");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "Data",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "EmailSent",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "EmailSentAt",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "ExpiresAt",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "RealTimeSent",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "RealTimeSentAt",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "NotificationPreferences");

            migrationBuilder.DropColumn(
                name: "NotificationSound",
                table: "NotificationPreferences");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "NotificationPreferences");

            migrationBuilder.RenameColumn(
                name: "EnableSystemNotifications",
                table: "NotificationPreferences",
                newName: "ReceiveSupportNotifications");

            migrationBuilder.RenameColumn(
                name: "EnableSupportNotifications",
                table: "NotificationPreferences",
                newName: "ReceiveReviewNotifications");

            migrationBuilder.RenameColumn(
                name: "EnableSound",
                table: "NotificationPreferences",
                newName: "ReceiveProductionNotifications");

            migrationBuilder.RenameColumn(
                name: "EnableRealTimeNotifications",
                table: "NotificationPreferences",
                newName: "ReceiveProductNotifications");

            migrationBuilder.RenameColumn(
                name: "EnableProductNotifications",
                table: "NotificationPreferences",
                newName: "ReceivePayoutNotifications");

            migrationBuilder.RenameColumn(
                name: "EnableOrderNotifications",
                table: "NotificationPreferences",
                newName: "ReceivePaymentNotifications");

            migrationBuilder.RenameColumn(
                name: "EnableFinancialNotifications",
                table: "NotificationPreferences",
                newName: "ReceiveOrderNotifications");

            migrationBuilder.RenameColumn(
                name: "EmailSystemNotifications",
                table: "NotificationPreferences",
                newName: "ReceivePush");

            migrationBuilder.RenameColumn(
                name: "EmailSupportNotifications",
                table: "NotificationPreferences",
                newName: "ReceiveInApp");

            migrationBuilder.RenameColumn(
                name: "EmailProductNotifications",
                table: "NotificationPreferences",
                newName: "ReceiveEmail");

            migrationBuilder.RenameColumn(
                name: "EmailOrderNotifications",
                table: "NotificationPreferences",
                newName: "ReceiveComplaintNotifications");

            migrationBuilder.RenameColumn(
                name: "EmailFinancialNotifications",
                table: "NotificationPreferences",
                newName: "ReceiveAutoBlockNotifications");

            migrationBuilder.RenameIndex(
                name: "IX_NotificationPreferences_UserId",
                table: "NotificationPreferences",
                newName: "IX_NotificationPreference_UserId_Unique");

            // ═══════════════════════════════════════════════════════════
            // NOTIFICATIONS TABLE
            // ═══════════════════════════════════════════════════════════

            migrationBuilder.AlterColumn<string>(
                name: "RelatedEntityType",
                table: "Notifications",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Priority",
                table: "Notifications",
                type: "int",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 2);

            migrationBuilder.AlterColumn<string>(
                name: "Message",
                table: "Notifications",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000);

            migrationBuilder.AlterColumn<string>(
                name: "ActionText",
                table: "Notifications",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            // ═══════════════════════════════════════════════════════════
            // NOTIFICATION PREFERENCES TABLE
            // ═══════════════════════════════════════════════════════════

            // ✅ ALTER CreatedAt with default value SQL
            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "NotificationPreferences",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "NotificationPreferences",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            // ✅ Add default values to renamed boolean columns
            migrationBuilder.AlterColumn<bool>(
                name: "ReceiveSupportNotifications",
                table: "NotificationPreferences",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "ReceiveReviewNotifications",
                table: "NotificationPreferences",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "ReceiveProductionNotifications",
                table: "NotificationPreferences",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "ReceiveProductNotifications",
                table: "NotificationPreferences",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "ReceivePayoutNotifications",
                table: "NotificationPreferences",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "ReceivePaymentNotifications",
                table: "NotificationPreferences",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "ReceiveOrderNotifications",
                table: "NotificationPreferences",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "ReceiveInApp",
                table: "NotificationPreferences",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "ReceiveEmail",
                table: "NotificationPreferences",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "ReceiveComplaintNotifications",
                table: "NotificationPreferences",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "ReceiveAutoBlockNotifications",
                table: "NotificationPreferences",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "ReceivePush",
                table: "NotificationPreferences",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            // ✅ Add new columns
            migrationBuilder.AddColumn<int>(
                name: "MinimumPriority",
                table: "NotificationPreferences",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "ReceiveAccountNotifications",
                table: "NotificationPreferences",
                type: "bit",
                nullable: false,
                defaultValue: true);

            // ═══════════════════════════════════════════════════════════
            // INDEXES
            // ═══════════════════════════════════════════════════════════

            migrationBuilder.CreateIndex(
                name: "IX_Notification_RelatedEntity",
                table: "Notifications",
                columns: new[] { "RelatedEntityType", "RelatedEntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_Notification_UserId",
                table: "Notifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Notification_UserId_CreatedAt",
                table: "Notifications",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Notification_UserId_IsRead",
                table: "Notifications",
                columns: new[] { "UserId", "IsRead" });

            migrationBuilder.CreateIndex(
                name: "IX_NotificationPreferences_UserId_CreatedAt",
                table: "NotificationPreferences",
                columns: new[] { "UserId", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Notification_RelatedEntity",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notification_UserId",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notification_UserId_CreatedAt",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notification_UserId_IsRead",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_NotificationPreferences_UserId_CreatedAt",
                table: "NotificationPreferences");

            migrationBuilder.DropColumn(
                name: "MinimumPriority",
                table: "NotificationPreferences");

            migrationBuilder.DropColumn(
                name: "ReceiveAccountNotifications",
                table: "NotificationPreferences");

            // Reverse AlterColumn for renamed columns
            migrationBuilder.AlterColumn<bool>(
                name: "ReceiveSupportNotifications",
                table: "NotificationPreferences",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<bool>(
                name: "ReceiveReviewNotifications",
                table: "NotificationPreferences",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<bool>(
                name: "ReceiveProductionNotifications",
                table: "NotificationPreferences",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<bool>(
                name: "ReceiveProductNotifications",
                table: "NotificationPreferences",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<bool>(
                name: "ReceivePayoutNotifications",
                table: "NotificationPreferences",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<bool>(
                name: "ReceivePaymentNotifications",
                table: "NotificationPreferences",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<bool>(
                name: "ReceiveOrderNotifications",
                table: "NotificationPreferences",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<bool>(
                name: "ReceiveInApp",
                table: "NotificationPreferences",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<bool>(
                name: "ReceiveEmail",
                table: "NotificationPreferences",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<bool>(
                name: "ReceiveComplaintNotifications",
                table: "NotificationPreferences",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<bool>(
                name: "ReceiveAutoBlockNotifications",
                table: "NotificationPreferences",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<bool>(
                name: "ReceivePush",
                table: "NotificationPreferences",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            // Reverse RenameColumn
            migrationBuilder.RenameColumn(
                name: "ReceiveSupportNotifications",
                table: "NotificationPreferences",
                newName: "EnableSystemNotifications");

            migrationBuilder.RenameColumn(
                name: "ReceiveReviewNotifications",
                table: "NotificationPreferences",
                newName: "EnableSupportNotifications");

            migrationBuilder.RenameColumn(
                name: "ReceiveProductionNotifications",
                table: "NotificationPreferences",
                newName: "EnableSound");

            migrationBuilder.RenameColumn(
                name: "ReceiveProductNotifications",
                table: "NotificationPreferences",
                newName: "EnableRealTimeNotifications");

            migrationBuilder.RenameColumn(
                name: "ReceivePayoutNotifications",
                table: "NotificationPreferences",
                newName: "EnableProductNotifications");

            migrationBuilder.RenameColumn(
                name: "ReceivePaymentNotifications",
                table: "NotificationPreferences",
                newName: "EnableOrderNotifications");

            migrationBuilder.RenameColumn(
                name: "ReceiveOrderNotifications",
                table: "NotificationPreferences",
                newName: "EnableFinancialNotifications");

            migrationBuilder.RenameColumn(
                name: "ReceivePush",
                table: "NotificationPreferences",
                newName: "EmailSystemNotifications");

            migrationBuilder.RenameColumn(
                name: "ReceiveInApp",
                table: "NotificationPreferences",
                newName: "EmailSupportNotifications");

            migrationBuilder.RenameColumn(
                name: "ReceiveEmail",
                table: "NotificationPreferences",
                newName: "EmailProductNotifications");

            migrationBuilder.RenameColumn(
                name: "ReceiveComplaintNotifications",
                table: "NotificationPreferences",
                newName: "EmailOrderNotifications");

            migrationBuilder.RenameColumn(
                name: "ReceiveAutoBlockNotifications",
                table: "NotificationPreferences",
                newName: "EmailFinancialNotifications");

            migrationBuilder.RenameIndex(
                name: "IX_NotificationPreference_UserId_Unique",
                table: "NotificationPreferences",
                newName: "IX_NotificationPreferences_UserId");

            // Reverse AlterColumns for Notifications
            migrationBuilder.AlterColumn<string>(
                name: "RelatedEntityType",
                table: "Notifications",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Priority",
                table: "Notifications",
                type: "int",
                nullable: false,
                defaultValue: 2,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 1);

            migrationBuilder.AlterColumn<string>(
                name: "Message",
                table: "Notifications",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "ActionText",
                table: "Notifications",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "NotificationPreferences",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "NotificationPreferences",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Notifications",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Data",
                table: "Notifications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EmailSent",
                table: "Notifications",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "EmailSentAt",
                table: "Notifications",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiresAt",
                table: "Notifications",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RealTimeSent",
                table: "Notifications",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "RealTimeSentAt",
                table: "Notifications",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Notifications",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Notifications",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "NotificationPreferences",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NotificationSound",
                table: "NotificationPreferences",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "default");

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "NotificationPreferences",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notification_CreatedBy_CreatedAt",
                table: "Notifications",
                columns: new[] { "CreatedBy", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_ExpiresAt",
                table: "Notifications",
                column: "ExpiresAt",
                filter: "[ExpiresAt] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_RelatedEntity",
                table: "Notifications",
                columns: new[] { "RelatedEntityType", "RelatedEntityId" },
                filter: "[RelatedEntityType] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId_IsRead_CreatedAt",
                table: "Notifications",
                columns: new[] { "UserId", "IsRead", "CreatedAt" })
                .Annotation("SqlServer:Include", new[] { "Type", "Title" });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId_Type",
                table: "Notifications",
                columns: new[] { "UserId", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_NotificationPreference_CreatedAt",
                table: "NotificationPreferences",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationPreference_CreatedBy_CreatedAt",
                table: "NotificationPreferences",
                columns: new[] { "CreatedBy", "CreatedAt" });
        }
    }
}