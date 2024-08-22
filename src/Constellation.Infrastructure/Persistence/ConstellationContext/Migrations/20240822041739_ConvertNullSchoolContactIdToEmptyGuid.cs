using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    /// <inheritdoc />
    public partial class ConvertNullSchoolContactIdToEmptyGuid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Absences_Absences_Offerings_Offerings_OfferingId",
                table: "Absences_Absences");

            migrationBuilder.DropForeignKey(
                name: "FK_Enrolments_Offerings_Offerings_OfferingId",
                table: "Enrolments");

            migrationBuilder.DropForeignKey(
                name: "FK_MSTeamOperations_SchoolContacts_Contacts_ContactId",
                table: "MSTeamOperations");

            migrationBuilder.DropForeignKey(
                name: "FK_Offerings_Offerings_Subjects_Courses_CourseId",
                table: "Offerings_Offerings");

            migrationBuilder.DropForeignKey(
                name: "FK_Offerings_Resources_Offerings_Offerings_OfferingId",
                table: "Offerings_Resources");

            migrationBuilder.DropForeignKey(
                name: "FK_Offerings_Sessions_Offerings_Offerings_OfferingId",
                table: "Offerings_Sessions");

            migrationBuilder.AlterColumn<Guid>(
                name: "FacultyId",
                table: "Subjects_Courses",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "LessonId",
                table: "SciencePracs_Rolls",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "SciencePracRollId",
                table: "SciencePracs_Attendance",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "SchoolContactId",
                table: "SchoolContacts_Roles",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "OfferingId",
                table: "Offerings_Teachers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "OfferingId",
                table: "Offerings_Sessions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "OfferingId",
                table: "Offerings_Resources",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "CourseId",
                table: "Offerings_Offerings",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "TutorialId",
                table: "GroupTutorials_Teachers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "TutorialId",
                table: "GroupTutorials_Roll",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "TutorialId",
                table: "GroupTutorials_Enrolment",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.DropIndex(
                name: "IX_FacultyMembership_FacultyId",
                table: "Faculties_Memberships");

            migrationBuilder.AlterColumn<Guid>(
                name: "FacultyId",
                table: "Faculties_Memberships",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FacultyMembership_FacultyId",
                table: "Faculties_Memberships",
                column: "FacultyId");

            migrationBuilder.AlterColumn<Guid>(
                name: "OfferingId",
                table: "Enrolments",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.DropIndex(
                name: "IX_Covers_ClassCovers_OfferignId",
                table: "Covers_ClassCovers");

            migrationBuilder.AlterColumn<Guid>(
                name: "OfferingId",
                table: "Covers_ClassCovers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Covers_ClassCovers_OfferingId",
                table: "Covers_ClassCovers",
                column: "OfferingId");

            migrationBuilder.AlterColumn<Guid>(
                name: "PeriodId",
                table: "Awards_Nominations",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "AssignmentId",
                table: "Assignments_Submissions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "CourseId",
                table: "Assignments_Assignments",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "SchoolContactId",
                table: "AspNetUsers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "OfferingId",
                table: "Absences_Absences",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Absences_Absences_Offerings_Offerings_OfferingId",
                table: "Absences_Absences",
                column: "OfferingId",
                principalTable: "Offerings_Offerings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Enrolments_Offerings_Offerings_OfferingId",
                table: "Enrolments",
                column: "OfferingId",
                principalTable: "Offerings_Offerings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MSTeamOperations_SchoolContacts_Contacts_ContactId",
                table: "MSTeamOperations",
                column: "ContactId",
                principalTable: "SchoolContacts_Contacts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Offerings_Offerings_Subjects_Courses_CourseId",
                table: "Offerings_Offerings",
                column: "CourseId",
                principalTable: "Subjects_Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Offerings_Resources_Offerings_Offerings_OfferingId",
                table: "Offerings_Resources",
                column: "OfferingId",
                principalTable: "Offerings_Offerings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Offerings_Sessions_Offerings_Offerings_OfferingId",
                table: "Offerings_Sessions",
                column: "OfferingId",
                principalTable: "Offerings_Offerings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Absences_Absences_Offerings_Offerings_OfferingId",
                table: "Absences_Absences");

            migrationBuilder.DropForeignKey(
                name: "FK_Enrolments_Offerings_Offerings_OfferingId",
                table: "Enrolments");

            migrationBuilder.DropForeignKey(
                name: "FK_MSTeamOperations_SchoolContacts_Contacts_ContactId",
                table: "MSTeamOperations");

            migrationBuilder.DropForeignKey(
                name: "FK_Offerings_Offerings_Subjects_Courses_CourseId",
                table: "Offerings_Offerings");

            migrationBuilder.DropForeignKey(
                name: "FK_Offerings_Resources_Offerings_Offerings_OfferingId",
                table: "Offerings_Resources");

            migrationBuilder.DropForeignKey(
                name: "FK_Offerings_Sessions_Offerings_Offerings_OfferingId",
                table: "Offerings_Sessions");

            migrationBuilder.AlterColumn<Guid>(
                name: "FacultyId",
                table: "Subjects_Courses",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "LessonId",
                table: "SciencePracs_Rolls",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "SciencePracRollId",
                table: "SciencePracs_Attendance",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "SchoolContactId",
                table: "SchoolContacts_Roles",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "OfferingId",
                table: "Offerings_Teachers",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "OfferingId",
                table: "Offerings_Sessions",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "OfferingId",
                table: "Offerings_Resources",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "CourseId",
                table: "Offerings_Offerings",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "TutorialId",
                table: "GroupTutorials_Teachers",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "TutorialId",
                table: "GroupTutorials_Roll",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "TutorialId",
                table: "GroupTutorials_Enrolment",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "FacultyId",
                table: "Faculties_Memberships",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "OfferingId",
                table: "Enrolments",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "OfferingId",
                table: "Covers_ClassCovers",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "PeriodId",
                table: "Awards_Nominations",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "AssignmentId",
                table: "Assignments_Submissions",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "CourseId",
                table: "Assignments_Assignments",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "SchoolContactId",
                table: "AspNetUsers",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "OfferingId",
                table: "Absences_Absences",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddForeignKey(
                name: "FK_Absences_Absences_Offerings_Offerings_OfferingId",
                table: "Absences_Absences",
                column: "OfferingId",
                principalTable: "Offerings_Offerings",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Enrolments_Offerings_Offerings_OfferingId",
                table: "Enrolments",
                column: "OfferingId",
                principalTable: "Offerings_Offerings",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MSTeamOperations_SchoolContacts_Contacts_ContactId",
                table: "MSTeamOperations",
                column: "ContactId",
                principalTable: "SchoolContacts_Contacts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Offerings_Offerings_Subjects_Courses_CourseId",
                table: "Offerings_Offerings",
                column: "CourseId",
                principalTable: "Subjects_Courses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Offerings_Resources_Offerings_Offerings_OfferingId",
                table: "Offerings_Resources",
                column: "OfferingId",
                principalTable: "Offerings_Offerings",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Offerings_Sessions_Offerings_Offerings_OfferingId",
                table: "Offerings_Sessions",
                column: "OfferingId",
                principalTable: "Offerings_Offerings",
                principalColumn: "Id");
        }
    }
}
