using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    /// <inheritdoc />
    public partial class UpdateQueuedItemSerialization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AlertRecipient",
                schema: "EmergencyConsole",
                table: "Queue",
                newName: "LastName");

            migrationBuilder.AddColumn<string>(
                name: "EmailAddress",
                schema: "EmergencyConsole",
                table: "Queue",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                schema: "EmergencyConsole",
                table: "Queue",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                schema: "EmergencyConsole",
                table: "Queue",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreferredName",
                schema: "EmergencyConsole",
                table: "Queue",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailAddress",
                schema: "EmergencyConsole",
                table: "Queue");

            migrationBuilder.DropColumn(
                name: "FirstName",
                schema: "EmergencyConsole",
                table: "Queue");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                schema: "EmergencyConsole",
                table: "Queue");

            migrationBuilder.DropColumn(
                name: "PreferredName",
                schema: "EmergencyConsole",
                table: "Queue");

            migrationBuilder.RenameColumn(
                name: "LastName",
                schema: "EmergencyConsole",
                table: "Queue",
                newName: "AlertRecipient");
        }
    }
}
