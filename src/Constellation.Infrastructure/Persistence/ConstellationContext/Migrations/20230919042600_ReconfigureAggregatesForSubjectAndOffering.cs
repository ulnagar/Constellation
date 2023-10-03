using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    using OfficeOpenXml.Style;
    using Templates.Views.Emails.Contacts;

    /// <inheritdoc />
    public partial class ReconfigureAggregatesForSubjectAndOffering : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop Indexes
            migrationBuilder.DropIndex(
                name: "IX_Subjects_Offerings_CourseId",
                table: "Subjects_Offerings"); // recreated with new name on line 439

            migrationBuilder.DropIndex(
                name: "IX_Subjects_Resources_OfferingId",
                table: "Subjects_Resources"); // recreated with new name on line 572

            migrationBuilder.DropIndex(
                name: "IX_Subjects_Sessions_OfferingId",
                table: "Subjects_Sessions"); // recreated with new name on line 577

            migrationBuilder.DropIndex(
                name: "IX_Subjects_Sessions_PeriodId",
                table: "Subjects_Sessions"); // recreated with new name on line 582

            migrationBuilder.DropIndex( // no longer required
                name: "IX_Subjects_Sessions_RoomId",
                table: "Subjects_Sessions");

            migrationBuilder.DropIndex( // no longer required
                name: "IX_Subjects_Sessions_StaffId",
                table: "Subjects_Sessions");


            // Drop Foreign Keys
            migrationBuilder.DropForeignKey(
                name: "FK_Subjects_Offerings_Subjects_Courses_CourseId",
                table: "Subjects_Offerings"); // recreated with new name on line 444

            migrationBuilder.DropForeignKey(
                name: "FK_Subjects_Resources_Subjects_Offerings_OfferingId",
                table: "Subjects_Resources"); // recreated with new name on line 154

            migrationBuilder.DropForeignKey(
                name: "FK_Subjects_Sessions_Periods_PeriodId",
                table: "Subjects_Sessions"); // recreated with new name on line 321

            migrationBuilder.DropForeignKey(
                name: "FK_Subjects_Sessions_Rooms_RoomId",
                table: "Subjects_Sessions"); // no longer required

            migrationBuilder.DropForeignKey(
                name: "FK_Subjects_Sessions_Staff_StaffId",
                table: "Subjects_Sessions"); // no longer required

            migrationBuilder.DropForeignKey(
                name: "FK_Subjects_Sessions_Subjects_Offerings_OfferingId",
                table: "Subjects_Sessions"); // recreated with new name on line 328

            migrationBuilder.DropForeignKey(
                name: "FK_Absences_Absences_Subjects_Offerings_OfferingId",
                table: "Absences_Absences"); // recreated with new name on line 597

            migrationBuilder.DropForeignKey(
                name: "FK_Covers_ClassCovers_Subjects_Offerings_OfferingId",
                table: "Covers_ClassCovers"); // recreated with new name on line 604

            migrationBuilder.DropForeignKey(
                name: "FK_Enrolments_Subjects_Offerings_OfferingId",
                table: "Enrolments"); // recreated with new name on line 612

            migrationBuilder.DropForeignKey(
                name: "FK_MSTeamOperations_Students_StudentId",
                table: "MSTeamOperations");

            migrationBuilder.DropForeignKey(
                name: "FK_MSTeamOperations_Subjects_Offerings_OfferingId",
                table: "MSTeamOperations"); // recreated with new name on line 619

            migrationBuilder.DropForeignKey(
                name: "FK_PartialAbsences_Subjects_Offerings_OfferingId",
                table: "PartialAbsences"); // recreated with new name on line 647

            migrationBuilder.DropForeignKey(
                name: "FK_SciencePracs_Lessons_Offerings_Subjects_Offerings_OfferingId",
                table: "SciencePracs_Lessons_Offerings"); // recreated with new name on line 654

            migrationBuilder.DropForeignKey(
                name: "FK_WholeAbsences_Subjects_Offerings_OfferingId",
                table: "WholeAbsences"); // recreated with new name on line 662

            migrationBuilder.DropForeignKey(
                name: "FK_Assignments_Assignments_Subjects_Courses_CourseId",
                table: "Assignments_Assignments"); // recreated for new column type on line 409


            // Drop Primary Keys
            migrationBuilder.DropPrimaryKey(
                name: "PK_Subjects_Offerings",
                table: "Subjects_Offerings"); // recreated with new name on line 124

            migrationBuilder.DropPrimaryKey(
                name: "PK_Subjects_Resources",
                table: "Subjects_Resources"); // recreated with new name on line 149
            
            migrationBuilder.DropPrimaryKey(
                name: "PK_Subjects_Sessions",
                table: "Subjects_Sessions"); // recreated with new name on line 133


            // Rename Tables
            migrationBuilder.RenameTable(
                name: "Subjects_Offerings",
                newName: "Offerings_Offerings");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Offerings_Offerings",
                table: "Offerings_Offerings",
                column: "Id");

            migrationBuilder.RenameTable(
                name: "Subjects_Sessions",
                newName: "Offerings_Sessions");

            migrationBuilder.RenameTable(
                name: "Subjects_Resources",
                newName: "Offerings_Resources");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Offerings_Resources",
                table: "Offerings_Resources",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Offerings_Resources_Offerings_Offerings_OfferingId",
                table: "Offerings_Resources",
                column: "OfferingId",
                principalTable: "Offerings_Offerings",
                principalColumn: "Id");

            migrationBuilder.CreateTable(
                name: "Offerings_Teachers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OfferingId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StaffId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_Offerings_Teachers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Offerings_Teachers_Offerings_Offerings_OfferingId",
                        column: x => x.OfferingId,
                        principalTable: "Offerings_Offerings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Offerings_Teachers_Staff_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Staff",
                        principalColumn: "StaffId");
                });

            // move all staff in the sessions table to separate records in the teachers table
            migrationBuilder.Sql(
                @"INSERT INTO [dbo].[Offerings_Teachers]
	([Id],
	 [OfferingId],
	 [StaffId],
	 [Type],
	 [CreatedBy],
	 [CreatedAt],
	 [ModifiedBy],
	 [ModifiedAt],
	 [DeletedBy],
	 [DeletedAt],
	 [IsDeleted])
SELECT
	 NEWID() as Id,
	 t.OfferingId,
	 t.StaffId,
	 CASE
		WHEN (SELECT [Type] From [Periods] AS P WHERE t.[PeriodId] = p.[Id]) = 'OTHER' THEN 'Tutorial Teacher'
		ELSE 'Classroom Teacher'
		END as Type,
	 CreatedBy = 'System',
	 CreatedAt = t.DateCreated,
	 ModifiedBy = null,
	 ModifiedAt = cast('0001-01-01' as datetime2),
	 DeletedBy = CASE
		WHEN t.IsDeleted = 0 THEN null
		WHEN t.IsDeleted = 1 THEN 'System'
		END,
	 DeletedAt = CASE
		WHEN t.IsDeleted = 0 THEN cast('0001-01-01' as datetime2)
		WHEN t.IsDeleted = 1 THEN t.DateDeleted
		END,
	 t.IsDeleted
FROM [dbo].[Offerings_Sessions] t
WHERE 
	(SELECT COUNT(Id)
	FROM [dbo].[Offerings_Sessions] j
	WHERE
		t.Id < j.Id and
		t.OfferingId = j.OfferingId and
		t.StaffId = j.StaffId and
		CASE
			WHEN (SELECT [Type] From [Periods] AS P WHERE j.[PeriodId] = p.[Id]) = 'OTHER' THEN 'Tutorial Teacher'
			ELSE 'Classroom Teacher'
			END =
		CASE
			WHEN (SELECT [Type] From [Periods] AS P WHERE t.[PeriodId] = p.[Id]) = 'OTHER' THEN 'Tutorial Teacher'
			ELSE 'Classroom Teacher'
			END) = 0");

            // Move all Rooms in the Sessions table to separate records in the Resources table
            migrationBuilder.Sql(
                @"INSERT INTO [dbo].[Offerings_Resources]
	([Id],
	 [Type],
	 [OfferingId],
	 [ResourceId],
	 [Name],
	 [Url])
SELECT
	 NEWID() as Id,
	 'Adobe Connect Room' as Type,
	 t.OfferingId,
	 t.RoomId,
	 r.Name,
	 r.UrlPath
FROM [dbo].[Offerings_Sessions] t
INNER JOIN [dbo].[Rooms] r on t.RoomId = r.ScoId
WHERE 
	(SELECT COUNT(Id)
	FROM [dbo].[Offerings_Sessions] j
	WHERE
		t.Id < j.Id and
		t.OfferingId = j.OfferingId and
		t.RoomId = j.RoomId) = 0");

            // Modify the Sessions table to remove extra columns
            migrationBuilder.DropColumn(
                name: "StaffId",
                table: "Offerings_Sessions"); // no longer required

            migrationBuilder.DropColumn(
                name: "RoomId",
                table: "Offerings_Sessions"); // no longer required

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Offerings_Sessions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.RenameColumn(
                name: "DateCreated",
                table: "Offerings_Sessions",
                newName: "CreatedAt");

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "Offerings_Sessions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedAt",
                table: "Offerings_Sessions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "Offerings_Sessions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.RenameColumn(
                name: "DateDeleted",
                table: "Offerings_Sessions",
                newName: "DeletedAt");
            
            migrationBuilder.AddForeignKey(
                name: "FK_Offerings_Sessions_Periods_PeriodId",
                table: "Offerings_Sessions",
                column: "PeriodId",
                principalTable: "Periods",
                principalColumn: "Id");

            migrationBuilder.Sql(@"
UPDATE [CONSTELLATION_DEV].[dbo].[Offerings_Sessions]
SET
	IsDeleted = 1,
	DeletedAt = CURRENT_TIMESTAMP, 
	DeletedBy = 'System'
FROM [CONSTELLATION_DEV].[dbo].[Offerings_Sessions] s
	LEFT JOIN [CONSTELLATION_DEV].dbo.Periods p on s.PeriodId = p.Id
WHERE
	p.Type = 'Other' and 
	s.IsDeleted = 0");

            // Migrate the Course.Id column from int to guid

            migrationBuilder.DropPrimaryKey(
                name: "PK_Subjects_Courses",
                table: "Subjects_Courses");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Subjects_Courses",
                newName: "xId");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "Subjects_Courses",
                type: "uniqueidentifier",
                nullable: true);

            // Generate new IDs for Courses
            migrationBuilder.Sql(
                @"UPDATE [dbo].[Subjects_Courses]
                    SET [Id] = NEWID()
                    WHERE 1 = 1;");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Subjects_Courses",
                nullable: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Subjects_Courses",
                table: "Subjects_Courses",
                column: "Id");

            // Update Awards_Nominations table for new CourseId type and value
            migrationBuilder.RenameColumn(
                name: "CourseId",
                table: "Awards_Nominations",
                newName: "xCourseId");

            migrationBuilder.AddColumn<Guid>(
                name: "CourseId",
                table: "Awards_Nominations",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql(
                @"UPDATE [dbo].[Awards_Nominations]
         SET [CourseId] = [Subjects_Courses].[Id]
         FROM [dbo].[Awards_Nominations]
         INNER JOIN [dbo].[Subjects_Courses]
         ON [dbo].[Awards_Nominations].[xCourseId] = [Subjects_Courses].[xId];");

            migrationBuilder.DropColumn(
                name: "xCourseId",
                table: "Awards_Nominations");
            
            // Update Assignments_Assignments table for new CourseId type and value
            migrationBuilder.RenameColumn(
                name: "CourseId",
                table: "Assignments_Assignments",
                newName: "xCourseId");

            migrationBuilder.AddColumn<Guid>(
                name: "CourseId",
                table: "Assignments_Assignments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql(
                @"UPDATE [dbo].[Assignments_Assignments]
         SET [CourseId] = [Subjects_Courses].[Id]
         FROM [dbo].[Assignments_Assignments]
         INNER JOIN [dbo].[Subjects_Courses]
         ON [dbo].[Assignments_Assignments].[xCourseId] = [Subjects_Courses].[xId];");

            migrationBuilder.DropIndex(
                name: "IX_Assignments_Assignments_CourseId",
                table: "Assignments_Assignments");

            migrationBuilder.DropColumn(
                name: "xCourseId",
                table: "Assignments_Assignments");

            migrationBuilder.CreateIndex(
                name: "IX_Assignments_Assignments_CourseId",
                table: "Assignments_Assignments",
                column: "CourseId");

            migrationBuilder.AddForeignKey(
                name: "FK_Assignments_Assignments_Subjects_Courses_CourseId",
                table: "Assignments_Assignments",
                column: "CourseId",
                principalTable: "Subjects_Courses",
                principalColumn: "Id");

            // Update Offerings_Offerings table for new CourseId type and value
            migrationBuilder.RenameColumn(
                name: "CourseId",
                table: "Offerings_Offerings",
                newName: "xCourseId");

            migrationBuilder.AddColumn<Guid>(
                name: "CourseId",
                table: "Offerings_Offerings",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql(
                @"UPDATE [dbo].[Offerings_Offerings]
         SET [CourseId] = [Subjects_Courses].[Id]
         FROM [dbo].[Offerings_Offerings]
         INNER JOIN [dbo].[Subjects_Courses]
         ON [dbo].[Offerings_Offerings].[xCourseId] = [Subjects_Courses].[xId];");

            migrationBuilder.DropColumn(
                name: "xCourseId",
                table: "Offerings_Offerings");

            migrationBuilder.CreateIndex(
                name: "IX_Offerings_Offerings_CourseId",
                table: "Offerings_Offerings",
                column: "CourseId");

            migrationBuilder.AddForeignKey(
                name: "FK_Offerings_Offerings_Subjects_Courses_CourseId",
                table: "Offerings_Offerings",
                column: "CourseId",
                principalTable: "Subjects_Courses",
                principalColumn: "Id");

            migrationBuilder.DropColumn(
                name: "xId",
                table: "Subjects_Courses");

            // Update Sessions table
            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Offerings_Sessions",
                newName: "xId");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "Offerings_Sessions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql(
                @"UPDATE [dbo].[Offerings_Sessions]
                    SET [Id] = NEWID()
                    WHERE 1 = 1;");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Offerings_Sessions",
                nullable: false);

            migrationBuilder.DropColumn(
                name: "xId",
                table: "Offerings_Sessions");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Offerings_Sessions",
                table: "Offerings_Sessions",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Offerings_Sessions_Offerings_Offerings_OfferingId",
                table: "Offerings_Sessions",
                column: "OfferingId",
                principalTable: "Offerings_Offerings",
                principalColumn: "Id");

            // Update Enrolments table
            migrationBuilder.DropPrimaryKey(
                name: "PK_Enrolments",
                table: "Enrolments"); // recreated for new column type on line 481

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Enrolments",
                newName: "xId");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "Enrolments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql(
                @"UPDATE [dbo].[Enrolments]
                    SET [Id] = NEWID()
                    WHERE 1 = 1;");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Enrolments",
                nullable: false);

            migrationBuilder.DropColumn(
                name: "xId",
                table: "Enrolments");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Enrolments",
                table: "Enrolments",
                column: "Id");

            migrationBuilder.Sql(
                @"UPDATE [dbo].[Enrolments]
                    SET [DateCreated] = Cast('01-01-0001' as datetime2)
                    WHERE DateCreated is null;");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateCreated",
                table: "Enrolments",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.RenameColumn(
                name: "DateCreated",
                table: "Enrolments",
                newName: "CreatedAt");

            migrationBuilder.Sql(
                @"UPDATE [dbo].[Enrolments]
                    SET [DateDeleted] = Cast('01-01-0001' as datetime2)
                    WHERE DateDeleted is null;");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateDeleted",
                table: "Enrolments",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.RenameColumn(
                name: "DateDeleted",
                table: "Enrolments",
                newName: "DeletedAt");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Enrolments",
                type: "nvarchar(max)",
                nullable: true);
            
            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "Enrolments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedAt",
                table: "Enrolments",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "Enrolments",
                type: "nvarchar(max)",
                nullable: true);
            
            
            // Process other changes
            migrationBuilder.RenameIndex(
                name: "IX_MSTeamOperations_StudentId",
                table: "MSTeamOperations",
                newName: "IX_MSTeamOperations_StudentId1");

            migrationBuilder.AddColumn<string>(
                name: "StudentEnrolledMSTeamOperation_StudentId",
                table: "MSTeamOperations",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TeacherEmployedMSTeamOperation_StaffId",
                table: "MSTeamOperations",
                type: "nvarchar(450)",
                nullable: true);



            migrationBuilder.CreateIndex(
                name: "IX_MSTeamOperations_StudentEnrolledMSTeamOperation_StudentId",
                table: "MSTeamOperations",
                column: "StudentEnrolledMSTeamOperation_StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_MSTeamOperations_TeacherEmployedMSTeamOperation_StaffId",
                table: "MSTeamOperations",
                column: "TeacherEmployedMSTeamOperation_StaffId");



            migrationBuilder.CreateIndex(
                name: "IX_Offerings_Resources_OfferingId",
                table: "Offerings_Resources",
                column: "OfferingId");

            migrationBuilder.CreateIndex(
                name: "IX_Offerings_Sessions_OfferingId",
                table: "Offerings_Sessions",
                column: "OfferingId");

            migrationBuilder.CreateIndex(
                name: "IX_Offerings_Sessions_PeriodId",
                table: "Offerings_Sessions",
                column: "PeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_Offerings_Teachers_OfferingId",
                table: "Offerings_Teachers",
                column: "OfferingId");

            migrationBuilder.CreateIndex(
                name: "IX_Offerings_Teachers_StaffId",
                table: "Offerings_Teachers",
                column: "StaffId");

            migrationBuilder.AddForeignKey(
                name: "FK_Absences_Absences_Offerings_Offerings_OfferingId",
                table: "Absences_Absences",
                column: "OfferingId",
                principalTable: "Offerings_Offerings",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Covers_ClassCovers_Offerings_Offerings_OfferingId",
                table: "Covers_ClassCovers",
                column: "OfferingId",
                principalTable: "Offerings_Offerings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Enrolments_Offerings_Offerings_OfferingId",
                table: "Enrolments",
                column: "OfferingId",
                principalTable: "Offerings_Offerings",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MSTeamOperations_Offerings_Offerings_OfferingId",
                table: "MSTeamOperations",
                column: "OfferingId",
                principalTable: "Offerings_Offerings",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MSTeamOperations_Staff_TeacherEmployedMSTeamOperation_StaffId",
                table: "MSTeamOperations",
                column: "TeacherEmployedMSTeamOperation_StaffId",
                principalTable: "Staff",
                principalColumn: "StaffId");

            migrationBuilder.AddForeignKey(
                name: "FK_MSTeamOperations_Students_StudentEnrolledMSTeamOperation_StudentId",
                table: "MSTeamOperations",
                column: "StudentEnrolledMSTeamOperation_StudentId",
                principalTable: "Students",
                principalColumn: "StudentId");

            migrationBuilder.AddForeignKey(
                name: "FK_MSTeamOperations_Students_StudentId1",
                table: "MSTeamOperations",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "StudentId");

            migrationBuilder.AddForeignKey(
                name: "FK_PartialAbsences_Offerings_Offerings_OfferingId",
                table: "PartialAbsences",
                column: "OfferingId",
                principalTable: "Offerings_Offerings",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SciencePracs_Lessons_Offerings_Offerings_Offerings_OfferingId",
                table: "SciencePracs_Lessons_Offerings",
                column: "OfferingId",
                principalTable: "Offerings_Offerings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WholeAbsences_Offerings_Offerings_OfferingId",
                table: "WholeAbsences",
                column: "OfferingId",
                principalTable: "Offerings_Offerings",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Absences_Absences_Offerings_Offerings_OfferingId",
                table: "Absences_Absences");

            migrationBuilder.DropForeignKey(
                name: "FK_Covers_ClassCovers_Offerings_Offerings_OfferingId",
                table: "Covers_ClassCovers");

            migrationBuilder.DropForeignKey(
                name: "FK_Enrolments_Offerings_Offerings_OfferingId",
                table: "Enrolments");

            migrationBuilder.DropForeignKey(
                name: "FK_MSTeamOperations_Offerings_Offerings_OfferingId",
                table: "MSTeamOperations");

            migrationBuilder.DropForeignKey(
                name: "FK_MSTeamOperations_Staff_TeacherEmployedMSTeamOperation_StaffId",
                table: "MSTeamOperations");

            migrationBuilder.DropForeignKey(
                name: "FK_MSTeamOperations_Students_StudentEnrolledMSTeamOperation_StudentId",
                table: "MSTeamOperations");

            migrationBuilder.DropForeignKey(
                name: "FK_MSTeamOperations_Students_StudentId1",
                table: "MSTeamOperations");

            migrationBuilder.DropForeignKey(
                name: "FK_PartialAbsences_Offerings_Offerings_OfferingId",
                table: "PartialAbsences");

            migrationBuilder.DropForeignKey(
                name: "FK_SciencePracs_Lessons_Offerings_Offerings_Offerings_OfferingId",
                table: "SciencePracs_Lessons_Offerings");

            migrationBuilder.DropForeignKey(
                name: "FK_WholeAbsences_Offerings_Offerings_OfferingId",
                table: "WholeAbsences");

            migrationBuilder.DropTable(
                name: "Offerings_Resources");

            migrationBuilder.DropTable(
                name: "Offerings_Sessions");

            migrationBuilder.DropTable(
                name: "Offerings_Teachers");

            migrationBuilder.DropTable(
                name: "Offerings_Offerings");

            migrationBuilder.DropIndex(
                name: "IX_MSTeamOperations_StudentEnrolledMSTeamOperation_StudentId",
                table: "MSTeamOperations");

            migrationBuilder.DropIndex(
                name: "IX_MSTeamOperations_TeacherEmployedMSTeamOperation_StaffId",
                table: "MSTeamOperations");

            migrationBuilder.DropColumn(
                name: "StudentEnrolledMSTeamOperation_StudentId",
                table: "MSTeamOperations");

            migrationBuilder.DropColumn(
                name: "TeacherEmployedMSTeamOperation_StaffId",
                table: "MSTeamOperations");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Enrolments");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Enrolments");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Enrolments");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Enrolments");

            migrationBuilder.DropColumn(
                name: "ModifiedAt",
                table: "Enrolments");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "Enrolments");

            migrationBuilder.RenameIndex(
                name: "IX_MSTeamOperations_StudentId1",
                table: "MSTeamOperations",
                newName: "IX_MSTeamOperations_StudentId");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Subjects_Courses",
                type: "int",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier")
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Enrolments",
                type: "int",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier")
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateCreated",
                table: "Enrolments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateDeleted",
                table: "Enrolments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CourseId",
                table: "Awards_Nominations",
                type: "int",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CourseId",
                table: "Assignments_Assignments",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "Subjects_Offerings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CourseId = table.Column<int>(type: "int", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subjects_Offerings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Subjects_Offerings_Subjects_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Subjects_Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Subjects_Resources",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OfferingId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResourceId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subjects_Resources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Subjects_Resources_Subjects_Offerings_OfferingId",
                        column: x => x.OfferingId,
                        principalTable: "Subjects_Offerings",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Subjects_Sessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OfferingId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PeriodId = table.Column<int>(type: "int", nullable: false),
                    RoomId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    StaffId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateDeleted = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subjects_Sessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Subjects_Sessions_Periods_PeriodId",
                        column: x => x.PeriodId,
                        principalTable: "Periods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Subjects_Sessions_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Rooms",
                        principalColumn: "ScoId");
                    table.ForeignKey(
                        name: "FK_Subjects_Sessions_Staff_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Staff",
                        principalColumn: "StaffId");
                    table.ForeignKey(
                        name: "FK_Subjects_Sessions_Subjects_Offerings_OfferingId",
                        column: x => x.OfferingId,
                        principalTable: "Subjects_Offerings",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_Offerings_CourseId",
                table: "Subjects_Offerings",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_Resources_OfferingId",
                table: "Subjects_Resources",
                column: "OfferingId");

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_Sessions_OfferingId",
                table: "Subjects_Sessions",
                column: "OfferingId");

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_Sessions_PeriodId",
                table: "Subjects_Sessions",
                column: "PeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_Sessions_RoomId",
                table: "Subjects_Sessions",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_Sessions_StaffId",
                table: "Subjects_Sessions",
                column: "StaffId");

            migrationBuilder.AddForeignKey(
                name: "FK_Absences_Absences_Subjects_Offerings_OfferingId",
                table: "Absences_Absences",
                column: "OfferingId",
                principalTable: "Subjects_Offerings",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Covers_ClassCovers_Subjects_Offerings_OfferingId",
                table: "Covers_ClassCovers",
                column: "OfferingId",
                principalTable: "Subjects_Offerings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Enrolments_Subjects_Offerings_OfferingId",
                table: "Enrolments",
                column: "OfferingId",
                principalTable: "Subjects_Offerings",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MSTeamOperations_Students_StudentId",
                table: "MSTeamOperations",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "StudentId");

            migrationBuilder.AddForeignKey(
                name: "FK_MSTeamOperations_Subjects_Offerings_OfferingId",
                table: "MSTeamOperations",
                column: "OfferingId",
                principalTable: "Subjects_Offerings",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PartialAbsences_Subjects_Offerings_OfferingId",
                table: "PartialAbsences",
                column: "OfferingId",
                principalTable: "Subjects_Offerings",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SciencePracs_Lessons_Offerings_Subjects_Offerings_OfferingId",
                table: "SciencePracs_Lessons_Offerings",
                column: "OfferingId",
                principalTable: "Subjects_Offerings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WholeAbsences_Subjects_Offerings_OfferingId",
                table: "WholeAbsences",
                column: "OfferingId",
                principalTable: "Subjects_Offerings",
                principalColumn: "Id");
        }
    }
}
