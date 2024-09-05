using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    /// <inheritdoc />
    public partial class ConvertStudentToAggregate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Absences_Absences_Students_StudentId",
                table: "Absences_Absences");

            migrationBuilder.DropForeignKey(
                name: "FK_Awards_StudentAwards_Students_StudentId",
                table: "Awards_StudentAwards");

            migrationBuilder.DropForeignKey(
                name: "FK_DeviceAllocations_Students_StudentId",
                table: "DeviceAllocations");

            migrationBuilder.DropForeignKey(
                name: "FK_Enrolments_Students_StudentId",
                table: "Enrolments");

            migrationBuilder.DropForeignKey(
                name: "FK_MSTeamOperations_Students_StudentEnrolledMSTeamOperation_StudentId",
                table: "MSTeamOperations");

            migrationBuilder.DropForeignKey(
                name: "FK_SciencePracs_Attendance_Students_StudentId",
                table: "SciencePracs_Attendance");

            migrationBuilder.DropForeignKey(
                name: "FK_ThirdParty_Transactions_Students_StudentId",
                table: "ThirdParty_Transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_ThirdParty_Consents_Students_StudentId",
                table: "ThirdParty_Consents");

            migrationBuilder.DropForeignKey(
                name: "FK_Students_AbsenceConfiguration_Students_StudentId",
                table: "Students_AbsenceConfiguration");

            migrationBuilder.DropColumn(
                name: "AbsenceNotificationStartDate",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "AwardTally_Astras",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "AwardTally_GalaxyMedals",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "AwardTally_Stellars",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "AwardTally_UniversalAchievers",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "CurrentGrade",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "DateDeleted",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "DateEntered",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "EnrolledGrade",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "IncludeInAbsenceNotifications",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "Photo",
                table: "Students");

            migrationBuilder.EnsureSchema(
                name: "Students");

            migrationBuilder.RenameTable(
                name: "Students",
                newName: "Students",
                newSchema: "Students");

            migrationBuilder.RenameColumn(
                name: "StudentId",
                schema: "Students",
                table: "Students",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "SentralStudentId",
                schema: "Students",
                table: "Students",
                newName: "PreferredGender");

            migrationBuilder.RenameColumn(
                name: "PortalUsername",
                schema: "Students",
                table: "Students",
                newName: "ModifiedBy");

            migrationBuilder.RenameColumn(
                name: "LastName",
                schema: "Students",
                table: "Students",
                newName: "EmailAddress");

            migrationBuilder.RenameColumn(
                name: "FirstName",
                schema: "Students",
                table: "Students",
                newName: "DeletedBy");

            migrationBuilder.RenameColumn(
                name: "AdobeConnectPrincipalId",
                schema: "Students",
                table: "Students",
                newName: "CreatedBy");

            migrationBuilder.AlterColumn<Guid>(
                name: "StudentId",
                table: "WorkFlows_CaseDetails",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "StudentId",
                table: "ThirdParty_Transactions",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<Guid>(
                name: "StudentId",
                table: "ThirdParty_Consents",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<Guid>(
                name: "StudentId",
                table: "Students_AbsenceConfiguration",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "StudentId",
                table: "SciencePracs_Attendance",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "StudentId",
                table: "Reports_AcademicReports",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "StudentMSTeamOperation_StudentId",
                table: "MSTeamOperations",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "StudentId",
                table: "MSTeamOperations",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "StudentEnrolledMSTeamOperation_StudentId",
                table: "MSTeamOperations",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "StudentId",
                table: "GroupTutorials_RollStudent",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<Guid>(
                name: "StudentId",
                table: "GroupTutorials_Enrolment",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "StudentId",
                table: "Families_StudentMemberships",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<Guid>(
                name: "StudentId1",
                table: "Families_StudentMemberships",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "StudentId",
                table: "Enrolments",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "StudentId",
                table: "DeviceAllocations",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdateUserEmailCanvasOperation_PortalUsername",
                table: "CanvasOperations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "StudentId",
                table: "Awards_StudentAwards",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "StudentId",
                table: "Awards_Nominations",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "StudentId",
                table: "Attendance_Values",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<Guid>(
                name: "StudentId",
                table: "Assignments_Submissions",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<Guid>(
                name: "StudentId",
                table: "AspNetUsers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "StudentId",
                table: "Absences_Absences",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Gender",
                schema: "Students",
                table: "Students",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1)",
                oldMaxLength: 1,
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                schema: "Students",
                table: "Students",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                schema: "Students",
                table: "Students",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                schema: "Students",
                table: "Students",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedAt",
                schema: "Students",
                table: "Students",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "AwardTallies",
                schema: "Students",
                columns: table => new
                {
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Astras = table.Column<int>(type: "int", nullable: false),
                    Stellars = table.Column<int>(type: "int", nullable: false),
                    GalaxyMedals = table.Column<int>(type: "int", nullable: false),
                    UniversalAchievers = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AwardTallies", x => x.StudentId);
                    table.ForeignKey(
                        name: "FK_AwardTallies_Students_StudentId",
                        column: x => x.StudentId,
                        principalSchema: "Students",
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SchoolEnrolments",
                schema: "Students",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SchoolCode = table.Column<string>(type: "nvarchar(4)", nullable: true),
                    SchoolName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Grade = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchoolEnrolments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SchoolEnrolments_Schools_SchoolCode",
                        column: x => x.SchoolCode,
                        principalTable: "Schools",
                        principalColumn: "Code");
                    table.ForeignKey(
                        name: "FK_SchoolEnrolments_Students_StudentId",
                        column: x => x.StudentId,
                        principalSchema: "Students",
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SystemLinks",
                schema: "Students",
                columns: table => new
                {
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    System = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemLinks", x => new { x.StudentId, x.System });
                    table.ForeignKey(
                        name: "FK_SystemLinks_Students_StudentId",
                        column: x => x.StudentId,
                        principalSchema: "Students",
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Families_StudentMemberships_StudentId1",
                table: "Families_StudentMemberships",
                column: "StudentId1");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolEnrolments_SchoolCode",
                schema: "Students",
                table: "SchoolEnrolments",
                column: "SchoolCode");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolEnrolments_StudentId",
                schema: "Students",
                table: "SchoolEnrolments",
                column: "StudentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Absences_Absences_Students_StudentId",
                table: "Absences_Absences",
                column: "StudentId",
                principalSchema: "Students",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Awards_StudentAwards_Students_StudentId",
                table: "Awards_StudentAwards",
                column: "StudentId",
                principalSchema: "Students",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceAllocations_Students_StudentId",
                table: "DeviceAllocations",
                column: "StudentId",
                principalSchema: "Students",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Enrolments_Students_StudentId",
                table: "Enrolments",
                column: "StudentId",
                principalSchema: "Students",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Families_StudentMemberships_Students_StudentId1",
                table: "Families_StudentMemberships",
                column: "StudentId1",
                principalSchema: "Students",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MSTeamOperations_Students_StudentEnrolledMSTeamOperation_StudentId",
                table: "MSTeamOperations",
                column: "StudentEnrolledMSTeamOperation_StudentId",
                principalSchema: "Students",
                principalTable: "Students",
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Absences_Absences_Students_StudentId",
                table: "Absences_Absences");

            migrationBuilder.DropForeignKey(
                name: "FK_Awards_StudentAwards_Students_StudentId",
                table: "Awards_StudentAwards");

            migrationBuilder.DropForeignKey(
                name: "FK_DeviceAllocations_Students_StudentId",
                table: "DeviceAllocations");

            migrationBuilder.DropForeignKey(
                name: "FK_Enrolments_Students_StudentId",
                table: "Enrolments");

            migrationBuilder.DropForeignKey(
                name: "FK_Families_StudentMemberships_Students_StudentId1",
                table: "Families_StudentMemberships");

            migrationBuilder.DropForeignKey(
                name: "FK_MSTeamOperations_Students_StudentEnrolledMSTeamOperation_StudentId",
                table: "MSTeamOperations");

            migrationBuilder.DropForeignKey(
                name: "FK_SciencePracs_Attendance_Students_StudentId",
                table: "SciencePracs_Attendance");

            migrationBuilder.DropTable(
                name: "AwardTallies",
                schema: "Students");

            migrationBuilder.DropTable(
                name: "SchoolEnrolments",
                schema: "Students");

            migrationBuilder.DropTable(
                name: "SystemLinks",
                schema: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Families_StudentMemberships_StudentId1",
                table: "Families_StudentMemberships");

            migrationBuilder.DropColumn(
                name: "StudentId1",
                table: "Families_StudentMemberships");

            migrationBuilder.DropColumn(
                name: "UpdateUserEmailCanvasOperation_PortalUsername",
                table: "CanvasOperations");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                schema: "Students",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                schema: "Students",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "ModifiedAt",
                schema: "Students",
                table: "Students");

            migrationBuilder.RenameTable(
                name: "Students",
                schema: "Students",
                newName: "Students");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Students",
                newName: "StudentId");

            migrationBuilder.RenameColumn(
                name: "PreferredGender",
                table: "Students",
                newName: "SentralStudentId");

            migrationBuilder.RenameColumn(
                name: "ModifiedBy",
                table: "Students",
                newName: "PortalUsername");

            migrationBuilder.RenameColumn(
                name: "EmailAddress",
                table: "Students",
                newName: "LastName");

            migrationBuilder.RenameColumn(
                name: "DeletedBy",
                table: "Students",
                newName: "FirstName");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "Students",
                newName: "AdobeConnectPrincipalId");

            migrationBuilder.AlterColumn<string>(
                name: "StudentId",
                table: "WorkFlows_CaseDetails",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "StudentId",
                table: "ThirdParty_Transactions",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "StudentId",
                table: "ThirdParty_Consents",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "StudentId",
                table: "Students_AbsenceConfiguration",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "StudentId",
                table: "SciencePracs_Attendance",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "StudentId",
                table: "Reports_AcademicReports",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "StudentMSTeamOperation_StudentId",
                table: "MSTeamOperations",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "StudentId",
                table: "MSTeamOperations",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "StudentEnrolledMSTeamOperation_StudentId",
                table: "MSTeamOperations",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "StudentId",
                table: "GroupTutorials_RollStudent",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "StudentId",
                table: "GroupTutorials_Enrolment",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "StudentId",
                table: "Families_StudentMemberships",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "StudentId",
                table: "Enrolments",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "StudentId",
                table: "DeviceAllocations",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "StudentId",
                table: "Awards_StudentAwards",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "StudentId",
                table: "Awards_Nominations",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "StudentId",
                table: "Attendance_Values",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "StudentId",
                table: "Assignments_Submissions",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "StudentId",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "StudentId",
                table: "Absences_Absences",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "Gender",
                table: "Students",
                type: "nvarchar(1)",
                maxLength: 1,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "StudentId",
                table: "Students",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<DateTime>(
                name: "AbsenceNotificationStartDate",
                table: "Students",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AwardTally_Astras",
                table: "Students",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AwardTally_GalaxyMedals",
                table: "Students",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AwardTally_Stellars",
                table: "Students",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AwardTally_UniversalAchievers",
                table: "Students",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CurrentGrade",
                table: "Students",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateDeleted",
                table: "Students",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateEntered",
                table: "Students",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EnrolledGrade",
                table: "Students",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IncludeInAbsenceNotifications",
                table: "Students",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<byte[]>(
                name: "Photo",
                table: "Students",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Absences_Absences_Students_StudentId",
                table: "Absences_Absences",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "StudentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Awards_StudentAwards_Students_StudentId",
                table: "Awards_StudentAwards",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "StudentId");

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceAllocations_Students_StudentId",
                table: "DeviceAllocations",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "StudentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Enrolments_Students_StudentId",
                table: "Enrolments",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "StudentId");

            migrationBuilder.AddForeignKey(
                name: "FK_MSTeamOperations_Students_StudentEnrolledMSTeamOperation_StudentId",
                table: "MSTeamOperations",
                column: "StudentEnrolledMSTeamOperation_StudentId",
                principalTable: "Students",
                principalColumn: "StudentId");

            migrationBuilder.AddForeignKey(
                name: "FK_SciencePracs_Attendance_Students_StudentId",
                table: "SciencePracs_Attendance",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "StudentId");
        }
    }
}
