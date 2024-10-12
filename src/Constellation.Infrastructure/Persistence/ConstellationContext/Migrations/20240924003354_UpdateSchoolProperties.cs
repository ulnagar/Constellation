using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSchoolProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Electorate",
                table: "Schools",
                newName: "EducationalServicesTeam");

            migrationBuilder.RenameColumn(
                name: "Division",
                table: "Schools",
                newName: "Directorate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EducationalServicesTeam",
                table: "Schools",
                newName: "Electorate");

            migrationBuilder.RenameColumn(
                name: "Directorate",
                table: "Schools",
                newName: "Division");
        }
    }
}
