using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    /// <inheritdoc />
    public partial class AddAwardNominations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Awards_NominationPeriods",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LockoutDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Awards_NominationPeriods", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Awards_NominationPeriods_Grades",
                columns: table => new
                {
                    PeriodId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Grade = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Awards_NominationPeriods_Grades", x => new { x.PeriodId, x.Grade });
                    table.ForeignKey(
                        name: "FK_Awards_NominationPeriods_Grades_Awards_NominationPeriods_PeriodId",
                        column: x => x.PeriodId,
                        principalTable: "Awards_NominationPeriods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Awards_Nominations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PeriodId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StudentId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    AwardType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CourseId = table.Column<int>(type: "int", nullable: true),
                    CourseName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OfferingId = table.Column<int>(type: "int", nullable: true),
                    ClassName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Awards_Nominations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Awards_Nominations_Awards_NominationPeriods_PeriodId",
                        column: x => x.PeriodId,
                        principalTable: "Awards_NominationPeriods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Awards_Nominations_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "StudentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Awards_Nominations_PeriodId",
                table: "Awards_Nominations",
                column: "PeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_Awards_Nominations_StudentId",
                table: "Awards_Nominations",
                column: "StudentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Awards_NominationPeriods_Grades");

            migrationBuilder.DropTable(
                name: "Awards_Nominations");

            migrationBuilder.DropTable(
                name: "Awards_NominationPeriods");
        }
    }
}
