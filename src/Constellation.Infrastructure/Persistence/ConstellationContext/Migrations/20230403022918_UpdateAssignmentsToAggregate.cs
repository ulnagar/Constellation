using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAssignmentsToAggregate : Migration
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

            migrationBuilder.RenameColumn(
                name: "SubmittedDate",
                table: "CanvasAssignmentsSubmissions",
                newName: "SubmittedOn");

            migrationBuilder.AlterColumn<string>(
                name: "StudentId",
                table: "CanvasAssignmentsSubmissions",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CourseId",
                table: "CanvasAssignments",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.RenameColumn(
                name: "SubmittedOn",
                table: "CanvasAssignmentsSubmissions",
                newName: "SubmittedDate");

            migrationBuilder.AlterColumn<string>(
                name: "StudentId",
                table: "CanvasAssignmentsSubmissions",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<int>(
                name: "CourseId",
                table: "CanvasAssignments",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_CanvasAssignments_Courses_CourseId",
                table: "CanvasAssignments",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_CanvasAssignmentsSubmissions_CanvasAssignments_AssignmentId",
                table: "CanvasAssignmentsSubmissions",
                column: "AssignmentId",
                principalTable: "CanvasAssignments",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_CanvasAssignmentsSubmissions_Students_StudentId",
                table: "CanvasAssignmentsSubmissions",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "StudentId",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
