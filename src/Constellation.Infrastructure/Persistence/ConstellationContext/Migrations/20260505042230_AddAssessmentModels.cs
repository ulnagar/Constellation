using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    /// <inheritdoc />
    public partial class AddAssessmentModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Assessments");

            migrationBuilder.CreateTable(
                name: "Assessments",
                schema: "Assessments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CourseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Course = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Grade = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DueDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    AvailableTo = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    AvailableFrom = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CanvasId = table.Column<int>(type: "int", nullable: false),
                    AllowedAttempts = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assessments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Assessments_Subjects_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Subjects_Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Provisions",
                schema: "Assessments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Provisions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Downloads",
                schema: "Assessments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssessmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AvailableFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    AvailableTo = table.Column<DateOnly>(type: "date", nullable: false),
                    IsRestricted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Downloads", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Downloads_Assessments_AssessmentId",
                        column: x => x.AssessmentId,
                        principalSchema: "Assessments",
                        principalTable: "Assessments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Instructions",
                schema: "Assessments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssessmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Details = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Instructions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Instructions_Assessments_AssessmentId",
                        column: x => x.AssessmentId,
                        principalSchema: "Assessments",
                        principalTable: "Assessments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Students",
                schema: "Assessments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssessmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentGrade = table.Column<int>(type: "int", nullable: false),
                    SchoolCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SchoolName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PreferredName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Students", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Students_Assessments_AssessmentId",
                        column: x => x.AssessmentId,
                        principalSchema: "Assessments",
                        principalTable: "Assessments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Students_Students_StudentId",
                        column: x => x.StudentId,
                        principalSchema: "Students",
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StudentProvisions",
                schema: "Assessments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProvisionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProvisionCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProvisionDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PreferredName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentProvisions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentProvisions_Provisions_ProvisionId",
                        column: x => x.ProvisionId,
                        principalSchema: "Assessments",
                        principalTable: "Provisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentProvisions_Students_StudentId",
                        column: x => x.StudentId,
                        principalSchema: "Students",
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DownloadEvents",
                schema: "Assessments",
                columns: table => new
                {
                    DownloadId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DownloadedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DownloadedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DownloadedByEmail = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DownloadEvents", x => new { x.DownloadId, x.UserId, x.DownloadedAt });
                    table.ForeignKey(
                        name: "FK_DownloadEvents_Downloads_DownloadId",
                        column: x => x.DownloadId,
                        principalSchema: "Assessments",
                        principalTable: "Downloads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AssessmentProvisions",
                schema: "Assessments",
                columns: table => new
                {
                    AssessmentStudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProvisionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssessmentProvisions", x => new { x.AssessmentStudentId, x.ProvisionId });
                    table.ForeignKey(
                        name: "FK_AssessmentProvisions_Students_AssessmentStudentId",
                        column: x => x.AssessmentStudentId,
                        principalSchema: "Assessments",
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Submissions",
                schema: "Assessments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssessmentStudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubmittedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    SubmittedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SubmittedByEmail = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Submissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Submissions_Students_AssessmentStudentId",
                        column: x => x.AssessmentStudentId,
                        principalSchema: "Assessments",
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Assessments_CourseId",
                schema: "Assessments",
                table: "Assessments",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_Downloads_AssessmentId",
                schema: "Assessments",
                table: "Downloads",
                column: "AssessmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Instructions_AssessmentId",
                schema: "Assessments",
                table: "Instructions",
                column: "AssessmentId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentProvisions_ProvisionId",
                schema: "Assessments",
                table: "StudentProvisions",
                column: "ProvisionId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentProvisions_StudentId",
                schema: "Assessments",
                table: "StudentProvisions",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentProvisions_Year_StudentId",
                schema: "Assessments",
                table: "StudentProvisions",
                columns: new[] { "Year", "StudentId" });

            migrationBuilder.CreateIndex(
                name: "IX_Students_AssessmentId",
                schema: "Assessments",
                table: "Students",
                column: "AssessmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Students_StudentId",
                schema: "Assessments",
                table: "Students",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_AssessmentStudentId",
                schema: "Assessments",
                table: "Submissions",
                column: "AssessmentStudentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssessmentProvisions",
                schema: "Assessments");

            migrationBuilder.DropTable(
                name: "DownloadEvents",
                schema: "Assessments");

            migrationBuilder.DropTable(
                name: "Instructions",
                schema: "Assessments");

            migrationBuilder.DropTable(
                name: "StudentProvisions",
                schema: "Assessments");

            migrationBuilder.DropTable(
                name: "Submissions",
                schema: "Assessments");

            migrationBuilder.DropTable(
                name: "Downloads",
                schema: "Assessments");

            migrationBuilder.DropTable(
                name: "Provisions",
                schema: "Assessments");

            migrationBuilder.DropTable(
                name: "Students",
                schema: "Assessments");

            migrationBuilder.DropTable(
                name: "Assessments",
                schema: "Assessments");
        }
    }
}
