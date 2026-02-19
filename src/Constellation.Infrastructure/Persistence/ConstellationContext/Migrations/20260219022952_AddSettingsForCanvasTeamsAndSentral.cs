using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    /// <inheritdoc />
    public partial class AddSettingsForCanvasTeamsAndSentral : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Absences",
                schema: "AppSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PartialLengthThreshold = table.Column<int>(type: "int", nullable: false),
                    ContactName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContactTitle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContactEmail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DiscountedWholeReasons = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DiscountedPartialReasons = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RollMarkingReportRecipients = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Absences", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Canvas",
                schema: "AppSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UseGroups = table.Column<bool>(type: "bit", nullable: false),
                    UseSections = table.Column<bool>(type: "bit", nullable: false),
                    Admins = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Canvas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Sentral",
                schema: "AppSettings",
                columns: table => new
                {
                    Type = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Path = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sentral", x => x.Type);
                });

            migrationBuilder.CreateTable(
                name: "Teams",
                schema: "AppSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MandatoryOwners = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StudentTeamOwners = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StudentChannelOwners = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teams", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Absences",
                schema: "AppSettings");

            migrationBuilder.DropTable(
                name: "Canvas",
                schema: "AppSettings");

            migrationBuilder.DropTable(
                name: "Sentral",
                schema: "AppSettings");

            migrationBuilder.DropTable(
                name: "Teams",
                schema: "AppSettings");
        }
    }
}
