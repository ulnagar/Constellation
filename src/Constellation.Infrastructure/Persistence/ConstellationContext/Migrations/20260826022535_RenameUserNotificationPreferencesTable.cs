using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    /// <inheritdoc />
    public partial class RenameUserNotificationPreferencesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppUserNotificationPreference_AspNetUsers_AppUserId",
                table: "AppUserNotificationPreference");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AppUserNotificationPreference",
                table: "AppUserNotificationPreference");

            migrationBuilder.RenameTable(
                name: "AppUserNotificationPreference",
                newName: "AspNetUserNotificationPreferences");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetUserNotificationPreferences",
                table: "AspNetUserNotificationPreferences",
                columns: new[] { "AppUserId", "NotificationType" });

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserNotificationPreferences_AspNetUsers_AppUserId",
                table: "AspNetUserNotificationPreferences",
                column: "AppUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserNotificationPreferences_AspNetUsers_AppUserId",
                table: "AspNetUserNotificationPreferences");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetUserNotificationPreferences",
                table: "AspNetUserNotificationPreferences");

            migrationBuilder.RenameTable(
                name: "AspNetUserNotificationPreferences",
                newName: "AppUserNotificationPreference");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AppUserNotificationPreference",
                table: "AppUserNotificationPreference",
                columns: new[] { "AppUserId", "NotificationType" });

            migrationBuilder.AddForeignKey(
                name: "FK_AppUserNotificationPreference_AspNetUsers_AppUserId",
                table: "AppUserNotificationPreference",
                column: "AppUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
