using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    /// <inheritdoc />
    public partial class SetCascadingDeleteFroSciencePracAggregate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SciencePracs_Attendance_SciencePracs_Rolls_SciencePracRollId",
                table: "SciencePracs_Attendance");

            migrationBuilder.DropForeignKey(
                name: "FK_SciencePracs_Rolls_SciencePracs_Lessons_LessonId",
                table: "SciencePracs_Rolls");

            migrationBuilder.AddForeignKey(
                name: "FK_SciencePracs_Attendance_SciencePracs_Rolls_SciencePracRollId",
                table: "SciencePracs_Attendance",
                column: "SciencePracRollId",
                principalTable: "SciencePracs_Rolls",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SciencePracs_Rolls_SciencePracs_Lessons_LessonId",
                table: "SciencePracs_Rolls",
                column: "LessonId",
                principalTable: "SciencePracs_Lessons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SciencePracs_Attendance_SciencePracs_Rolls_SciencePracRollId",
                table: "SciencePracs_Attendance");

            migrationBuilder.DropForeignKey(
                name: "FK_SciencePracs_Rolls_SciencePracs_Lessons_LessonId",
                table: "SciencePracs_Rolls");

            migrationBuilder.AddForeignKey(
                name: "FK_SciencePracs_Attendance_SciencePracs_Rolls_SciencePracRollId",
                table: "SciencePracs_Attendance",
                column: "SciencePracRollId",
                principalTable: "SciencePracs_Rolls",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SciencePracs_Rolls_SciencePracs_Lessons_LessonId",
                table: "SciencePracs_Rolls",
                column: "LessonId",
                principalTable: "SciencePracs_Lessons",
                principalColumn: "Id");
        }
    }
}
