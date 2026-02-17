using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    /// <inheritdoc />
    public partial class AddSettingsForWorkflowsTrainingAndTutorials : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MandatoryTraining",
                schema: "AppSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Contacts = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MandatoryTraining", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tutorials",
                schema: "AppSettings",
                columns: table => new
                {
                    PositionName = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Members = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tutorials", x => x.PositionName);
                });

            migrationBuilder.CreateTable(
                name: "Workflows",
                schema: "AppSettings",
                columns: table => new
                {
                    PositionName = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Members = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Workflows", x => x.PositionName);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MandatoryTraining",
                schema: "AppSettings");

            migrationBuilder.DropTable(
                name: "Tutorials",
                schema: "AppSettings");

            migrationBuilder.DropTable(
                name: "Workflows",
                schema: "AppSettings");
        }
    }
}
