using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    /// <inheritdoc />
    public partial class RenameAssignmentsTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CanvasAssignments_Courses_CourseId",
                table: "CanvasAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_CanvasAssignmentsSubmissions_CanvasAssignments_AssignmentId",
                table: "CanvasAssignmentsSubmissions");

            migrationBuilder.DropForeignKey(
                name: "FK_CanvasAssignmentsSubmissions_Students_StudentId",
                table: "CanvasAssignmentsSubmissions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CanvasAssignmentsSubmissions",
                table: "CanvasAssignmentsSubmissions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CanvasAssignments",
                table: "CanvasAssignments");

            migrationBuilder.RenameTable(
                name: "CanvasAssignmentsSubmissions",
                newName: "Assignments_Submissions");

            migrationBuilder.RenameTable(
                name: "CanvasAssignments",
                newName: "Assignments_Assignments");

            migrationBuilder.RenameIndex(
                name: "IX_CanvasAssignmentsSubmissions_StudentId",
                table: "Assignments_Submissions",
                newName: "IX_Assignments_Submissions_StudentId");

            migrationBuilder.RenameIndex(
                name: "IX_CanvasAssignmentsSubmissions_AssignmentId",
                table: "Assignments_Submissions",
                newName: "IX_Assignments_Submissions_AssignmentId");

            migrationBuilder.RenameIndex(
                name: "IX_CanvasAssignments_CourseId",
                table: "Assignments_Assignments",
                newName: "IX_Assignments_Assignments_CourseId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Assignments_Submissions",
                table: "Assignments_Submissions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Assignments_Assignments",
                table: "Assignments_Assignments",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Assignments_Assignments_Courses_CourseId",
                table: "Assignments_Assignments",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Assignments_Submissions_Assignments_Assignments_AssignmentId",
                table: "Assignments_Submissions",
                column: "AssignmentId",
                principalTable: "Assignments_Assignments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Assignments_Submissions_Students_StudentId",
                table: "Assignments_Submissions",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "StudentId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assignments_Assignments_Courses_CourseId",
                table: "Assignments_Assignments");

            migrationBuilder.DropForeignKey(
                name: "FK_Assignments_Submissions_Assignments_Assignments_AssignmentId",
                table: "Assignments_Submissions");

            migrationBuilder.DropForeignKey(
                name: "FK_Assignments_Submissions_Students_StudentId",
                table: "Assignments_Submissions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Assignments_Submissions",
                table: "Assignments_Submissions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Assignments_Assignments",
                table: "Assignments_Assignments");

            migrationBuilder.RenameTable(
                name: "Assignments_Submissions",
                newName: "CanvasAssignmentsSubmissions");

            migrationBuilder.RenameTable(
                name: "Assignments_Assignments",
                newName: "CanvasAssignments");

            migrationBuilder.RenameIndex(
                name: "IX_Assignments_Submissions_StudentId",
                table: "CanvasAssignmentsSubmissions",
                newName: "IX_CanvasAssignmentsSubmissions_StudentId");

            migrationBuilder.RenameIndex(
                name: "IX_Assignments_Submissions_AssignmentId",
                table: "CanvasAssignmentsSubmissions",
                newName: "IX_CanvasAssignmentsSubmissions_AssignmentId");

            migrationBuilder.RenameIndex(
                name: "IX_Assignments_Assignments_CourseId",
                table: "CanvasAssignments",
                newName: "IX_CanvasAssignments_CourseId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CanvasAssignmentsSubmissions",
                table: "CanvasAssignmentsSubmissions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CanvasAssignments",
                table: "CanvasAssignments",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CanvasAssignments_Courses_CourseId",
                table: "CanvasAssignments",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CanvasAssignmentsSubmissions_CanvasAssignments_AssignmentId",
                table: "CanvasAssignmentsSubmissions",
                column: "AssignmentId",
                principalTable: "CanvasAssignments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CanvasAssignmentsSubmissions_Students_StudentId",
                table: "CanvasAssignmentsSubmissions",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "StudentId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
