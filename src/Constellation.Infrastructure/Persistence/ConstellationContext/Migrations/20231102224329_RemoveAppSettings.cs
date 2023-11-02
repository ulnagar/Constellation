using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    /// <inheritdoc />
    public partial class RemoveAppSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppSettings");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    AdobeConnectDefaultFolder = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LessonsCoordinatorEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LessonsCoordinatorName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LessonsCoordinatorTitle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LessonsHeadTeacherEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SentralContactPreference = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Absences_AbsenceCoordinatorEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Absences_AbsenceCoordinatorName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Absences_AbsenceCoordinatorTitle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Absences_AbsenceScanStartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Absences_DiscountedPartialReasons = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Absences_DiscountedWholeReasons = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Absences_ForwardingEmailAbsenceCoordinator = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Absences_ForwardingEmailTruancyOfficer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Absences_PartialLengthThreshold = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppSettings", x => x.Id);
                });
        }
    }
}
