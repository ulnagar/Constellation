using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    /// <inheritdoc />
    public partial class ConvertSciencePracsToAggregate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CourseOfferingLesson_OfferingsId",
                table: "CourseOfferingLesson");

            migrationBuilder.DropIndex(
                name: "IX_LessonRolls_LessonId",
                table: "LessonRolls");

            migrationBuilder.DropIndex(
                name: "IX_LessonRolls_SchoolCode",
                table: "LessonRolls");

            migrationBuilder.DropIndex(
                name: "IX_LessonRolls_SchoolContactId",
                table: "LessonRolls");

            migrationBuilder.DropIndex(
                name: "IX_LessonRollStudentAttendance_LessonRollId",
                table: "LessonRollStudentAttendance");

            migrationBuilder.DropIndex(
                name: "IX_LessonRollStudentAttendance_StudentId",
                table: "LessonRollStudentAttendance");



            migrationBuilder.DropForeignKey(
                name: "FK_CourseOfferingLesson_Lessons_LessonsId",
                table: "CourseOfferingLesson");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseOfferingLesson_Offerings_OfferingsId",
                table: "CourseOfferingLesson");
            
            migrationBuilder.DropForeignKey(
                name: "FK_LessonRolls_Lessons_LessonId",
                table: "LessonRolls");

            migrationBuilder.DropForeignKey(
                name: "FK_LessonRolls_SchoolContact_SchoolContactId",
                table: "LessonRolls");

            migrationBuilder.DropForeignKey(
                name: "FK_LessonRolls_Schools_SchoolCode",
                table: "LessonRolls");

            migrationBuilder.DropForeignKey(
                name: "FK_LessonRollStudentAttendance_LessonRolls_LessonRollId",
                table: "LessonRollStudentAttendance");

            migrationBuilder.DropForeignKey(
                name: "FK_LessonRollStudentAttendance_Students_StudentId",
                table: "LessonRollStudentAttendance");



            migrationBuilder.DropPrimaryKey(
                name: "PK_CourseOfferingLesson",
                table: "CourseOfferingLesson");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LessonRolls",
                table: "LessonRolls");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LessonRollStudentAttendance",
                table: "LessonRollStudentAttendance");
            
            migrationBuilder.DropPrimaryKey(
                name: "PK_Lessons",
                table: "Lessons");



            migrationBuilder.RenameTable(
                name: "CourseOfferingLesson",
                newName: "SciencePracs_Lessons_Offerings");

            migrationBuilder.RenameColumn(
                name: "LessonsId",
                table: "SciencePracs_Lessons_Offerings",
                newName: "LessonId");

            migrationBuilder.RenameColumn(
                name: "OfferingsId",
                table: "SciencePracs_Lessons_Offerings",
                newName: "OfferingId");

            migrationBuilder.RenameTable(
                name: "LessonRolls",
                newName: "SciencePracs_Rolls");

            migrationBuilder.AddColumn<string>(
                name: "SubmittedBy",
                table: "SciencePracs_Rolls",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NotificationCount",
                table: "SciencePracs_Rolls",
                type: "int",
                nullable: false);

            migrationBuilder.RenameTable(
                name: "LessonRollStudentAttendance",
                newName: "SciencePracs_Attendance");

            migrationBuilder.RenameColumn(
                name: "LessonRollId",
                table: "SciencePracs_Attendance",
                newName: "SciencePracRollId");

            migrationBuilder.RenameTable(
                name: "Lessons",
                newName: "SciencePracs_Lessons");



            migrationBuilder.AddPrimaryKey(
                name: "PK_SciencePracs_Lessons_Offerings",
                table: "SciencePracs_Lessons_Offerings",
                columns: new string[] { "LessonId", "OfferingId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_SciencePracs_Rolls",
                table: "SciencePracs_Rolls",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SciencePracs_Attendance",
                table: "SciencePracs_Attendance",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SciencePracs_Lessons",
                table: "SciencePracs_Lessons",
                column: "Id");



            migrationBuilder.AddForeignKey(
                name: "FK_SciencePracs_Lessons_Offerings_Offerings_OfferingId",
                table: "SciencePracs_Lessons_Offerings",
                column: "OfferingId",
                principalTable: "Offerings",
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
                name: "FK_SciencePracs_Rolls_SchoolContact_SchoolContactId",
                table: "SciencePracs_Rolls",
                column: "SchoolContactId",
                principalTable: "SchoolContact",
                principalColumn: "Id");

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
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SciencePracs_Attendance_SciencePracs_Rolls_SciencePracRollId",
                table: "SciencePracs_Attendance",
                column: "SciencePracRollId",
                principalTable: "SciencePracs_Rolls",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SciencePracs_Attendance_Students_StudentId",
                table: "SciencePracs_Attendance",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "StudentId");



            migrationBuilder.CreateIndex(
                name: "IX_SciencePracs_Attendance_SciencePracRollId",
                table: "SciencePracs_Attendance",
                column: "SciencePracRollId");

            migrationBuilder.CreateIndex(
                name: "IX_SciencePracs_Attendance_StudentId",
                table: "SciencePracs_Attendance",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_SciencePracs_Lessons_Offerings_OfferingId",
                table: "SciencePracs_Lessons_Offerings",
                column: "OfferingId");

            migrationBuilder.CreateIndex(
                name: "IX_SciencePracs_Rolls_LessonId",
                table: "SciencePracs_Rolls",
                column: "LessonId");

            migrationBuilder.CreateIndex(
                name: "IX_SciencePracs_Rolls_SchoolCode",
                table: "SciencePracs_Rolls",
                column: "SchoolCode");

            migrationBuilder.CreateIndex(
                name: "IX_SciencePracs_Rolls_SchoolContactId",
                table: "SciencePracs_Rolls",
                column: "SchoolContactId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SciencePracs_Attendance_SciencePracRollId",
                table: "SciencePracs_Attendance");

            migrationBuilder.DropIndex(
                name: "IX_SciencePracs_Attendance_StudentId",
                table: "SciencePracs_Attendance");

            migrationBuilder.DropIndex(
                name: "IX_SciencePracs_Lessons_Offerings_OfferingId",
                table: "SciencePracs_Lessons_Offerings");

            migrationBuilder.DropIndex(
                name: "IX_SciencePracs_Rolls_LessonId",
                table: "SciencePracs_Rolls");

            migrationBuilder.DropIndex(
                name: "IX_SciencePracs_Rolls_SchoolCode",
                table: "SciencePracs_Rolls");

            migrationBuilder.DropIndex(
                name: "IX_SciencePracs_Rolls_SchoolContactId",
                table: "SciencePracs_Rolls");



            migrationBuilder.DropForeignKey(
                name: "FK_SciencePracs_Lessons_Offerings_Offerings_OfferingId",
                table: "SciencePracs_Lessons_Offerings");

            migrationBuilder.DropForeignKey(
                name: "FK_SciencePracs_Lessons_Offerings_SciencePracs_Lessons_LessonId",
                table: "SciencePracs_Lessons_Offerings");

            migrationBuilder.DropForeignKey(
                name: "FK_SciencePracs_Rolls_SchoolContact_SchoolContactId",
                table: "SciencePracs_Rolls");

            migrationBuilder.DropForeignKey(
                name: "FK_SciencePracs_Rolls_Schools_SchoolCode",
                table: "SciencePracs_Rolls");

            migrationBuilder.DropForeignKey(
                name: "FK_SciencePracs_Rolls_SciencePracs_Lessons_LessonId",
                table: "SciencePracs_Rolls");

            migrationBuilder.DropForeignKey(
                name: "FK_SciencePracs_Attendance_SciencePracs_Rolls_SciencePracRollId",
                table: "SciencePracs_Attendance");

            migrationBuilder.DropForeignKey(
                name: "FK_SciencePracs_Attendance_Students_StudentId",
                table: "SciencePracs_Attendance");



            migrationBuilder.DropPrimaryKey(
                name: "PK_SciencePracs_Lessons_Offerings",
                table: "SciencePracs_Lessons_Offerings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SciencePracs_Rolls",
                table: "SciencePracs_Rolls");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SciencePracs_Attendance",
                table: "SciencePracs_Attendance");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SciencePracs_Lessons",
                table: "SciencePracs_Lessons");



            

            migrationBuilder.RenameColumn(
                newName: "LessonsId",
                table: "SciencePracs_Lessons_Offerings",
                name: "LessonId");

            migrationBuilder.RenameColumn(
                newName: "OfferingsId",
                table: "SciencePracs_Lessons_Offerings",
                name: "OfferingId");

            migrationBuilder.RenameTable(
                newName: "CourseOfferingLesson",
                name: "SciencePracs_Lessons_Offerings");

            migrationBuilder.DropColumn(
                name: "SubmittedBy",
                table: "SciencePracs_Rolls");

            migrationBuilder.DropColumn(
                name: "NotificationCount",
                table: "SciencePracs_Rolls");

            migrationBuilder.RenameTable(
                newName: "LessonRolls",
                name: "SciencePracs_Rolls");

            migrationBuilder.RenameColumn(
                name: "LessonRollId",
                table: "SciencePracs_Attendance",
                newName: "SciencePracRollId");

            migrationBuilder.RenameTable(
                newName: "LessonRollStudentAttendance",
                name: "SciencePracs_Attendance");

            migrationBuilder.RenameTable(
                newName: "Lessons",
                name: "SciencePracs_Lessons");



            migrationBuilder.AddPrimaryKey(
                name: "PK_CourseOfferingLesson",
                table: "CourseOfferingLesson",
                columns: new string[] { "LessonsId", "OfferingsId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_LessonRolls",
                table: "LessonRolls",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LessonRollStudentAttendance",
                table: "LessonRollStudentAttendance",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Lessons",
                table: "Lessons",
                column: "Id");



            migrationBuilder.AddForeignKey(
                name: "FK_CourseOfferingLesson_Lessons_LessonsId",
                table: "CourseOfferingLesson",
                column: "LessonsId",
                principalTable: "Lessons",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CourseOfferingLesson_Offerings_OfferingsId",
                table: "CourseOfferingLesson",
                column: "OfferingsId",
                principalTable: "Offerings",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LessonRolls_Lessons_LessonId",
                table: "LessonRolls",
                column: "LessonId",
                principalTable: "Lessons",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LessonRolls_SchoolContact_SchoolContactId",
                table: "LessonRolls",
                column: "SchoolContactId",
                principalTable: "SchoolContact",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LessonRolls_Schools_SchoolCode",
                table: "LessonRolls",
                column: "SchoolCode",
                principalTable: "Schools",
                principalColumn: "Code");

            migrationBuilder.AddForeignKey(
                name: "FK_LessonRollStudentAttendance_LessonRolls_LessonRollId",
                table: "LessonRollStudentAttendance",
                column: "LessonRollId",
                principalTable: "LessonRolls",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LessonRollStudentAttendance_Students_StudentId",
                table: "LessonRollStudentAttendance",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "StudentId");



            migrationBuilder.CreateIndex(
                name: "IX_CourseOfferingLesson_OfferingsId",
                table: "CourseOfferingLesson",
                column: "OfferingsId");

            migrationBuilder.CreateIndex(
                name: "IX_LessonRolls_LessonId",
                table: "LessonRolls",
                column: "LessonId");

            migrationBuilder.CreateIndex(
                name: "IX_LessonRolls_SchoolCode",
                table: "LessonRolls",
                column: "SchoolCode");

            migrationBuilder.CreateIndex(
                name: "IX_LessonRolls_SchoolContactId",
                table: "LessonRolls",
                column: "SchoolContactId");

            migrationBuilder.CreateIndex(
                name: "IX_LessonRollStudentAttendance_LessonRollId",
                table: "LessonRollStudentAttendance",
                column: "LessonRollId");

            migrationBuilder.CreateIndex(
                name: "IX_LessonRollStudentAttendance_StudentId",
                table: "LessonRollStudentAttendance",
                column: "StudentId");
        }
    }
}
