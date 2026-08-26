using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    /// <inheritdoc />
    public partial class AddModuleToDraftsAndQueuedEmails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SendingModule",
                schema: "Messages",
                table: "Queue",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");


            migrationBuilder.AddColumn<string>(
                name: "SendingModule",
                schema: "Messages",
                table: "Drafts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(@"UPDATE Messages.Queue SET SendingModule = 'Messaging' WHERE 1=1;");
            migrationBuilder.Sql(@"UPDATE Messages.Drafts SET SendingModule = 'Messaging' WHERE 1=1;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SendingModule",
                schema: "Messages",
                table: "Queue");

            migrationBuilder.DropColumn(
                name: "SendingModule",
                schema: "Messages",
                table: "Drafts");
        }
    }
}
