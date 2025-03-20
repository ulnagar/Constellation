using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    /// <inheritdoc />
    public partial class AddGradeToSciencePracLesson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SciencePracs_Attendance_SciencePracs_Rolls_SciencePracRollId",
                table: "SciencePracs_Attendance");

            migrationBuilder.DropForeignKey(
                name: "FK_SciencePracs_Attendance_Students_StudentId",
                table: "SciencePracs_Attendance");

            migrationBuilder.DropForeignKey(
                name: "FK_SciencePracs_Lessons_Offerings_Offerings_Offerings_OfferingId",
                table: "SciencePracs_Lessons_Offerings");

            migrationBuilder.DropForeignKey(
                name: "FK_SciencePracs_Lessons_Offerings_SciencePracs_Lessons_LessonId",
                table: "SciencePracs_Lessons_Offerings");

            migrationBuilder.DropForeignKey(
                name: "FK_SciencePracs_Rolls_Schools_SchoolCode",
                table: "SciencePracs_Rolls");

            migrationBuilder.DropForeignKey(
                name: "FK_SciencePracs_Rolls_SciencePracs_Lessons_LessonId",
                table: "SciencePracs_Rolls");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SciencePracs_Rolls",
                table: "SciencePracs_Rolls");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SciencePracs_Lessons_Offerings",
                table: "SciencePracs_Lessons_Offerings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SciencePracs_Lessons",
                table: "SciencePracs_Lessons");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SciencePracs_Attendance",
                table: "SciencePracs_Attendance");

            migrationBuilder.EnsureSchema(
                name: "SciencePracs");

            migrationBuilder.RenameTable(
                name: "SciencePracs_Rolls",
                newName: "Rolls",
                newSchema: "SciencePracs");

            migrationBuilder.RenameTable(
                name: "SciencePracs_Lessons_Offerings",
                newName: "LessonOfferings",
                newSchema: "SciencePracs");

            migrationBuilder.RenameTable(
                name: "SciencePracs_Lessons",
                newName: "Lessons",
                newSchema: "SciencePracs");

            migrationBuilder.RenameTable(
                name: "SciencePracs_Attendance",
                newName: "Attendance",
                newSchema: "SciencePracs");

            migrationBuilder.RenameIndex(
                name: "IX_SciencePracs_Rolls_SchoolCode",
                schema: "SciencePracs",
                table: "Rolls",
                newName: "IX_Rolls_SchoolCode");

            migrationBuilder.RenameIndex(
                name: "IX_SciencePracs_Rolls_LessonId",
                schema: "SciencePracs",
                table: "Rolls",
                newName: "IX_Rolls_LessonId");

            migrationBuilder.RenameIndex(
                name: "IX_SciencePracs_Lessons_Offerings_OfferingId",
                schema: "SciencePracs",
                table: "LessonOfferings",
                newName: "IX_LessonOfferings_OfferingId");

            migrationBuilder.RenameIndex(
                name: "IX_SciencePracs_Attendance_StudentId",
                schema: "SciencePracs",
                table: "Attendance",
                newName: "IX_Attendance_StudentId");

            migrationBuilder.RenameIndex(
                name: "IX_SciencePracs_Attendance_SciencePracRollId",
                schema: "SciencePracs",
                table: "Attendance",
                newName: "IX_Attendance_SciencePracRollId");

            migrationBuilder.AddColumn<int>(
                name: "Grade",
                schema: "SciencePracs",
                table: "Lessons",
                type: "int",
                nullable: false,
                defaultValue: 13);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Rolls",
                schema: "SciencePracs",
                table: "Rolls",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LessonOfferings",
                schema: "SciencePracs",
                table: "LessonOfferings",
                columns: new[] { "LessonId", "OfferingId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Lessons",
                schema: "SciencePracs",
                table: "Lessons",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Attendance",
                schema: "SciencePracs",
                table: "Attendance",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Attendance_Rolls_SciencePracRollId",
                schema: "SciencePracs",
                table: "Attendance",
                column: "SciencePracRollId",
                principalSchema: "SciencePracs",
                principalTable: "Rolls",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Attendance_Students_StudentId",
                schema: "SciencePracs",
                table: "Attendance",
                column: "StudentId",
                principalSchema: "Students",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LessonOfferings_Lessons_LessonId",
                schema: "SciencePracs",
                table: "LessonOfferings",
                column: "LessonId",
                principalSchema: "SciencePracs",
                principalTable: "Lessons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LessonOfferings_Offerings_Offerings_OfferingId",
                schema: "SciencePracs",
                table: "LessonOfferings",
                column: "OfferingId",
                principalTable: "Offerings_Offerings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Rolls_Lessons_LessonId",
                schema: "SciencePracs",
                table: "Rolls",
                column: "LessonId",
                principalSchema: "SciencePracs",
                principalTable: "Lessons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Rolls_Schools_SchoolCode",
                schema: "SciencePracs",
                table: "Rolls",
                column: "SchoolCode",
                principalTable: "Schools",
                principalColumn: "Code");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attendance_Rolls_SciencePracRollId",
                schema: "SciencePracs",
                table: "Attendance");

            migrationBuilder.DropForeignKey(
                name: "FK_Attendance_Students_StudentId",
                schema: "SciencePracs",
                table: "Attendance");

            migrationBuilder.DropForeignKey(
                name: "FK_LessonOfferings_Lessons_LessonId",
                schema: "SciencePracs",
                table: "LessonOfferings");

            migrationBuilder.DropForeignKey(
                name: "FK_LessonOfferings_Offerings_Offerings_OfferingId",
                schema: "SciencePracs",
                table: "LessonOfferings");

            migrationBuilder.DropForeignKey(
                name: "FK_Rolls_Lessons_LessonId",
                schema: "SciencePracs",
                table: "Rolls");

            migrationBuilder.DropForeignKey(
                name: "FK_Rolls_Schools_SchoolCode",
                schema: "SciencePracs",
                table: "Rolls");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Rolls",
                schema: "SciencePracs",
                table: "Rolls");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Lessons",
                schema: "SciencePracs",
                table: "Lessons");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LessonOfferings",
                schema: "SciencePracs",
                table: "LessonOfferings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Attendance",
                schema: "SciencePracs",
                table: "Attendance");

            migrationBuilder.DropColumn(
                name: "Grade",
                schema: "SciencePracs",
                table: "Lessons");

            migrationBuilder.RenameTable(
                name: "Rolls",
                schema: "SciencePracs",
                newName: "SciencePracs_Rolls");

            migrationBuilder.RenameTable(
                name: "Lessons",
                schema: "SciencePracs",
                newName: "SciencePracs_Lessons");

            migrationBuilder.RenameTable(
                name: "LessonOfferings",
                schema: "SciencePracs",
                newName: "SciencePracs_Lessons_Offerings");

            migrationBuilder.RenameTable(
                name: "Attendance",
                schema: "SciencePracs",
                newName: "SciencePracs_Attendance");

            migrationBuilder.RenameIndex(
                name: "IX_Rolls_SchoolCode",
                table: "SciencePracs_Rolls",
                newName: "IX_SciencePracs_Rolls_SchoolCode");

            migrationBuilder.RenameIndex(
                name: "IX_Rolls_LessonId",
                table: "SciencePracs_Rolls",
                newName: "IX_SciencePracs_Rolls_LessonId");

            migrationBuilder.RenameIndex(
                name: "IX_LessonOfferings_OfferingId",
                table: "SciencePracs_Lessons_Offerings",
                newName: "IX_SciencePracs_Lessons_Offerings_OfferingId");

            migrationBuilder.RenameIndex(
                name: "IX_Attendance_StudentId",
                table: "SciencePracs_Attendance",
                newName: "IX_SciencePracs_Attendance_StudentId");

            migrationBuilder.RenameIndex(
                name: "IX_Attendance_SciencePracRollId",
                table: "SciencePracs_Attendance",
                newName: "IX_SciencePracs_Attendance_SciencePracRollId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SciencePracs_Rolls",
                table: "SciencePracs_Rolls",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SciencePracs_Lessons",
                table: "SciencePracs_Lessons",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SciencePracs_Lessons_Offerings",
                table: "SciencePracs_Lessons_Offerings",
                columns: new[] { "LessonId", "OfferingId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_SciencePracs_Attendance",
                table: "SciencePracs_Attendance",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SciencePracs_Attendance_SciencePracs_Rolls_SciencePracRollId",
                table: "SciencePracs_Attendance",
                column: "SciencePracRollId",
                principalTable: "SciencePracs_Rolls",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SciencePracs_Attendance_Students_StudentId",
                table: "SciencePracs_Attendance",
                column: "StudentId",
                principalSchema: "Students",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SciencePracs_Lessons_Offerings_Offerings_Offerings_OfferingId",
                table: "SciencePracs_Lessons_Offerings",
                column: "OfferingId",
                principalTable: "Offerings_Offerings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SciencePracs_Lessons_Offerings_SciencePracs_Lessons_LessonId",
                table: "SciencePracs_Lessons_Offerings",
                column: "LessonId",
                principalTable: "SciencePracs_Lessons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SciencePracs_Rolls_Schools_SchoolCode",
                table: "SciencePracs_Rolls",
                column: "SchoolCode",
                principalTable: "Schools",
                principalColumn: "Code");

            migrationBuilder.AddForeignKey(
                name: "FK_SciencePracs_Rolls_SciencePracs_Lessons_LessonId",
                table: "SciencePracs_Rolls",
                column: "LessonId",
                principalTable: "SciencePracs_Lessons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
