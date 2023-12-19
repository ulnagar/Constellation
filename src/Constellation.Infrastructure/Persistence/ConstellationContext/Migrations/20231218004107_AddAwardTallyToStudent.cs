using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    /// <inheritdoc />
    public partial class AddAwardTallyToStudent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AwardTally_Astras",
                table: "Students",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AwardTally_GalaxyMedals",
                table: "Students",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AwardTally_Stellars",
                table: "Students",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AwardTally_UniversalAchievers",
                table: "Students",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AwardTally_Astras",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "AwardTally_GalaxyMedals",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "AwardTally_Stellars",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "AwardTally_UniversalAchievers",
                table: "Students");
        }
    }
}
