using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    public partial class AddCanvasAssignmentTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CanvasAssignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CourseId = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CanvasId = table.Column<int>(type: "int", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LockDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UnlockDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AllowedAttempts = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CanvasAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CanvasAssignments_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "CanvasAssignmentsSubmissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssignmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StudentId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    SubmittedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Attempt = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CanvasAssignmentsSubmissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CanvasAssignmentsSubmissions_CanvasAssignments_AssignmentId",
                        column: x => x.AssignmentId,
                        principalTable: "CanvasAssignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CanvasAssignmentsSubmissions_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "StudentId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CanvasAssignments_CourseId",
                table: "CanvasAssignments",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_CanvasAssignmentsSubmissions_AssignmentId",
                table: "CanvasAssignmentsSubmissions",
                column: "AssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_CanvasAssignmentsSubmissions_StudentId",
                table: "CanvasAssignmentsSubmissions",
                column: "StudentId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CanvasAssignmentsSubmissions");

            migrationBuilder.DropTable(
                name: "CanvasAssignments");
        }
    }
}
