using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    public partial class ConvertTeamToAggregate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Teams",
                table: "Teams");

            migrationBuilder.RenameTable(
                name: "Teams",
                newName: "LinkedSystems_Teams");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "LinkedSystems_Teams",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "LinkedSystems_Teams",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_LinkedSystems_Teams",
                table: "LinkedSystems_Teams",
                column: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_LinkedSystems_Teams",
                table: "LinkedSystems_Teams");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "LinkedSystems_Teams");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "LinkedSystems_Teams");

            migrationBuilder.RenameTable(
                name: "LinkedSystems_Teams",
                newName: "Teams");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Teams",
                table: "Teams",
                column: "Id");
        }
    }
}
