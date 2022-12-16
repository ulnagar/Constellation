using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    public partial class UpdateOutboxImplementation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ProcessedOnUtc",
                table: "OutboxMessages",
                newName: "ProcessedOn");

            migrationBuilder.RenameColumn(
                name: "OccurredOnUtc",
                table: "OutboxMessages",
                newName: "OccurredOn");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ProcessedOn",
                table: "OutboxMessages",
                newName: "ProcessedOnUtc");

            migrationBuilder.RenameColumn(
                name: "OccurredOn",
                table: "OutboxMessages",
                newName: "OccurredOnUtc");
        }
    }
}
