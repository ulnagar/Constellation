using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    /// <inheritdoc />
    public partial class AddAttendancePlan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Attendance");

            migrationBuilder.CreateTable(
                name: "Plans",
                schema: "Attendance",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Grade = table.Column<int>(type: "int", nullable: false),
                    SchoolCode = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    School = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Student_FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Student_LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Student_PreferredName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Plans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Plans_Students_StudentId",
                        column: x => x.StudentId,
                        principalSchema: "Students",
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlanPeriods",
                schema: "Attendance",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PeriodId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Timetable = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Week = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Day = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PeriodName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PeriodType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    OfferingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OfferingName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CourseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CourseName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TargetMinutesPerCycle = table.Column<double>(type: "float(2)", precision: 2, nullable: false),
                    EntryTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    ExitTime = table.Column<TimeOnly>(type: "time", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanPeriods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlanPeriods_Offerings_Offerings_OfferingId",
                        column: x => x.OfferingId,
                        principalTable: "Offerings_Offerings",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PlanPeriods_Periods_PeriodId",
                        column: x => x.PeriodId,
                        principalSchema: "Timetables",
                        principalTable: "Periods",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PlanPeriods_Plans_PlanId",
                        column: x => x.PlanId,
                        principalSchema: "Attendance",
                        principalTable: "Plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlanPeriods_Subjects_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Subjects_Courses",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlanPeriods_CourseId",
                schema: "Attendance",
                table: "PlanPeriods",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanPeriods_OfferingId",
                schema: "Attendance",
                table: "PlanPeriods",
                column: "OfferingId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanPeriods_PeriodId",
                schema: "Attendance",
                table: "PlanPeriods",
                column: "PeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanPeriods_PlanId",
                schema: "Attendance",
                table: "PlanPeriods",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_Plans_StudentId",
                schema: "Attendance",
                table: "Plans",
                column: "StudentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlanPeriods",
                schema: "Attendance");

            migrationBuilder.DropTable(
                name: "Plans",
                schema: "Attendance");
        }
    }
}
