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
            migrationBuilder.EnsureSchema(
                name: "Students");

            migrationBuilder.CreateTable(
                name: "Students",
                schema: "Students",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentReferenceNumber = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Name_FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name_PreferredName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name_LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmailAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Gender = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PreferredGender = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SchoolCode = table.Column<string>(type: "nvarchar(4)", nullable: true),
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
                    table.PrimaryKey("PK_Students", x => x.Id);
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

            migrationBuilder.CreateTable(
                name: "AbsenceConfigurations",
                schema: "Students",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CalendarYear = table.Column<int>(type: "int", nullable: false),
                    AbsenceType = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ScanStartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ScanEndDate = table.Column<DateOnly>(type: "date", nullable: false),
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
                    table.PrimaryKey("PK_AbsenceConfigurations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AbsenceConfigurations_Students_StudentId",
                        column: x => x.StudentId,
                        principalSchema: "Students",
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "Students",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql(
                @"UPDATE [dbo].[Students]
                    SET [Id] = NEWID()
                    WHERE 1 = 1;");

            migrationBuilder.Sql(
                @"INSERT INTO [Students].[Students]
                  ([Id],
                   [StudentReferenceNumber],
                   [Name_FirstName],
                   [Name_PreferredName],
                   [Name_LastName],
                   [EmailAddress],
                   [Gender],
                   [PreferredGender],
                   [SchoolCode],
                   [CreatedBy],
                   [CreatedAt],
                   [ModifiedBy],
                   [ModifiedAt],
                   [IsDeleted],
                   [DeletedBy],
                   [DeletedAt])
                  SELECT 
                    Id,
                    StudentId,
                    FirstName,
                    FirstName,
                    LastName,
                    LTRIM(CONCAT(PortalUsername, '@education.nsw.gov.au')),
                    CASE 
                        WHEN Gender = 'm' THEN 'Male' 
                        ELSE 'Female'
                    END,
                    CASE 
                        WHEN Gender = 'M' THEN 'Male'
                        ELSE 'Female'
                    END,
                    SchoolCode,
                    'System Migration',
                    DateEntered,
                    NULL,
                    CAST('0001-01-01' as DateTime2),
                    IsDeleted,
                    CASE 
                        WHEN IsDeleted = '1' THEN 'System Migration'
                        ELSE NULL
					END,
                    CASE 
                        WHEN IsDeleted = '1' THEN DateDeleted
                        ELSE CAST('0001-01-01' as DateTime2)
					END
                  FROM [dbo].[Students]");

            migrationBuilder.Sql(
                @"INSERT INTO [Students].[SchoolEnrolments]
                   ([Id]
                   ,[StudentId]
                   ,[SchoolCode]
                   ,[SchoolName]
                   ,[Grade]
                   ,[Year]
                   ,[StartDate]
                   ,[EndDate]
                   ,[CreatedBy]
                   ,[CreatedAt]
                   ,[ModifiedBy]
                   ,[ModifiedAt]
                   ,[IsDeleted]
                   ,[DeletedBy]
                   ,[DeletedAt])
                SELECT 
			        NEWID(),
			        Id,
			        SchoolCode,
			        Schools.Name,
			        CurrentGrade,
			        '2024',
			        CASE
				        WHEN DateEntered < CAST('2024-01-01' as DateTime2) THEN CAST('2024-01-01' as DateTime2)
				        ELSE DateEntered
			        END,
			        NULL,
			        'System Migration',
			        CASE
				        WHEN DateEntered < CAST('2024-01-01' as DateTime2) THEN CAST('2024-01-01' as DateTime2)
				        ELSE DateEntered
			        END,
			        NULL,
			        CAST('0001-01-01' as DateTime2),
			        '0',
			        NULL,
			        CAST('0001-01-01' as DateTime2)
                FROM [dbo].[Students]
                JOIN [dbo].[Schools] on Students.SchoolCode = Schools.Code
		        WHERE IsDeleted = 0");

            migrationBuilder.Sql(
                @"INSERT INTO [Students].[SchoolEnrolments]
                   ([Id]
                   ,[StudentId]
                   ,[SchoolCode]
                   ,[SchoolName]
                   ,[Grade]
                   ,[Year]
                   ,[StartDate]
                   ,[EndDate]
                   ,[CreatedBy]
                   ,[CreatedAt]
                   ,[ModifiedBy]
                   ,[ModifiedAt]
                   ,[IsDeleted]
                   ,[DeletedBy]
                   ,[DeletedAt])
                SELECT 
			        NEWID(),
			        Id,
			        SchoolCode,
			        Schools.Name,
			        CurrentGrade,
			        YEAR(DateDeleted),
			        CAST(CONCAT(DATEPART(yyyy, DateDeleted), '-01-01') as datetime2),
			        DateDeleted,
			        'System Migration',
			        CAST(CONCAT(DATEPART(yyyy, DateDeleted), '-01-01') as datetime2),
			        NULL,
			        CAST('0001-01-01' as DateTime2),
			        1,
			        'System Migration',
			        DateDeleted
                FROM [dbo].[Students]
                JOIN [dbo].[Schools] on Students.SchoolCode = Schools.Code
		        WHERE IsDeleted = 1");

            migrationBuilder.Sql(
                @"INSERT INTO [Students].[AwardTallies]
                   ([StudentId]
                   ,[Astras]
                   ,[Stellars]
                   ,[GalaxyMedals]
                   ,[UniversalAchievers])
                SELECT 
			        Id,
			        AwardTally_Astras,
			        AwardTally_Stellars,
			        AwardTally_GalaxyMedals,
			        AwardTally_UniversalAchievers
                FROM [dbo].[Students]");

            migrationBuilder.Sql(
                @"INSERT INTO [Students].[SystemLinks]
                   ([StudentId]
                   ,[System]
                   ,[Value])
                 SELECT 
		            Id,
		            'Sentral',
		            SentralStudentId
	             FROM [dbo].[Students]
                 WHERE SentralStudentId is not NULL");

            migrationBuilder.Sql(
                @"INSERT INTO [Students].[AbsenceConfigurations]
                       ([Id]
                       ,[StudentId]
                       ,[CalendarYear]
                       ,[AbsenceType]
                       ,[ScanStartDate]
                       ,[ScanEndDate]
                       ,[CreatedBy]
                       ,[CreatedAt]
                       ,[ModifiedBy]
                       ,[ModifiedAt]
                       ,[IsDeleted]
                       ,[DeletedBy]
                       ,[DeletedAt])
	            SELECT
                       NEWID(),
                       Students.Id,
                       CalendarYear,
                       AbsenceType,
                       ScanStartDate,
                       ScanEndDate,
		               CreatedBy,
		               CreatedAt,
		               ModifiedBy,
		               ModifiedAt,
		               Students_AbsenceConfiguration.IsDeleted,
		               DeletedBy,
		               DeletedAt
	            FROM [dbo].[Students_AbsenceConfiguration]
	            JOIN Students on Students_AbsenceConfiguration.StudentId = Students.StudentId");

            // End custom code

            migrationBuilder.DropForeignKey(
                name: "FK_Absences_Absences_Students_StudentId",
                table: "Absences_Absences");

            migrationBuilder.DropForeignKey(
                name: "FK_Assignments_Submissions_Students_StudentId",
                table: "Assignments_Submissions");

            migrationBuilder.DropForeignKey(
                name: "FK_Attendance_Values_Students_StudentId",
                table: "Attendance_Values");

            migrationBuilder.DropForeignKey(
                name: "FK_Awards_Nominations_Students_StudentId",
                table: "Awards_Nominations");

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
                name: "FK_Families_StudentMemberships_Students_StudentId",
                table: "Families_StudentMemberships");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupTutorials_Enrolment_Students_StudentId",
                table: "GroupTutorials_Enrolment");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupTutorials_RollStudent_Students_StudentId",
                table: "GroupTutorials_RollStudent");

            migrationBuilder.DropForeignKey(
                name: "FK_Reports_AcademicReports_Students_StudentId",
                table: "Reports_AcademicReports");

            migrationBuilder.DropForeignKey(
                name: "FK_SciencePracs_Attendance_Students_StudentId",
                table: "SciencePracs_Attendance");

            migrationBuilder.DropForeignKey(
                name: "FK_Students_AbsenceConfiguration_Students_StudentId",
                table: "Students_AbsenceConfiguration");

            migrationBuilder.DropForeignKey(
                name: "FK_ThirdParty_Consents_Students_StudentId",
                table: "ThirdParty_Consents");

            migrationBuilder.DropForeignKey(
                name: "FK_ThirdParty_Transactions_Students_StudentId",
                table: "ThirdParty_Transactions");

            #region WorkFlows_CaseDetails

            migrationBuilder.RenameColumn(
                name: "StudentId",
                table: "WorkFlows_CaseDetails",
                newName: "xStudentId");

            migrationBuilder.AddColumn<Guid>(
                name: "StudentId",
                table: "WorkFlows_CaseDetails",
                type: "uniqueidentifier",
                nullable: true);
            
            migrationBuilder.Sql(
                @"UPDATE [dbo].[Workflows_CaseDetails]
                 SET [StudentId] = [Students].[Students].[Id]
                 FROM [Students].[Students]
                 INNER JOIN [dbo].[WorkFlows_CaseDetails]
                 ON [dbo].[Workflows_CaseDetails].[xStudentId] = [Students].[Students].[StudentReferenceNumber];");

            migrationBuilder.DropColumn(
                name: "xStudentId",
                table: "Workflows_CaseDetails");

            #endregion

            #region ThirdParty_Transactions

            migrationBuilder.DropIndex(
                name: "IX_ThirdParty_Transactions_StudentId",
                table: "ThirdParty_Transactions");

            migrationBuilder.RenameColumn(
                name: "StudentId",
                table: "ThirdParty_Transactions",
                newName: "xStudentId");

            migrationBuilder.AddColumn<Guid>(
                name: "StudentId",
                table: "ThirdParty_Transactions",
                type: "uniqueidentifier",
                nullable: false);

            migrationBuilder.Sql(
                @"UPDATE [dbo].[ThirdParty_Transactions]
                 SET [StudentId] = [Students].[Students].[Id]
                 FROM [Students].[Students]
                 INNER JOIN [dbo].[ThirdParty_Transactions]
                 ON [dbo].[ThirdParty_Transactions].[xStudentId] = [Students].[Students].[StudentReferenceNumber];");

            migrationBuilder.DropColumn(
                name: "xStudentId",
                table: "ThirdParty_Transactions");

            migrationBuilder.CreateIndex(
                name: "IX_ThirdParty_Transactions_StudentId",
                table: "ThirdParty_Transactions",
                column: "StudentId");

            #endregion

            #region ThirdParty_Consents

            migrationBuilder.DropIndex(
                name: "IX_ThirdParty_Consents_StudentId",
                table: "ThirdParty_Consents");

            migrationBuilder.RenameColumn(
                name: "StudentId",
                table: "ThirdParty_Consents",
                newName: "xStudentId");

            migrationBuilder.AddColumn<Guid>(
                name: "StudentId",
                table: "ThirdParty_Consents",
                type: "uniqueidentifier",
                nullable: false);

            migrationBuilder.Sql(
                @"UPDATE [dbo].[ThirdParty_Consents]
                 SET [StudentId] = [Students].[Students].[Id]
                 FROM [Students].[Students]
                 INNER JOIN [dbo].[ThirdParty_Consents]
                 ON [dbo].[ThirdParty_Consents].[xStudentId] = [Students].[Students].[StudentReferenceNumber];");

            migrationBuilder.DropColumn(
                name: "xStudentId",
                table: "ThirdParty_Consents");

            migrationBuilder.CreateIndex(
                name: "IX_ThirdParty_Consents_StudentId",
                table: "ThirdParty_Consents",
                column: "StudentId");

            #endregion

            #region SciencePracs_Attendance

            migrationBuilder.DropIndex(
                name: "IX_SciencePracs_Attendance_StudentId",
                table: "SciencePracs_Attendance");

            migrationBuilder.RenameColumn(
                name: "StudentId",
                table: "SciencePracs_Attendance",
                newName: "xStudentId");

            migrationBuilder.AddColumn<Guid>(
                name: "StudentId",
                table: "SciencePracs_Attendance",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.Sql(
                @"UPDATE [dbo].[SciencePracs_Attendance]
                 SET [StudentId] = [Students].[Students].[Id]
                 FROM [Students].[Students]
                 INNER JOIN [dbo].[SciencePracs_Attendance]
                 ON [dbo].[SciencePracs_Attendance].[xStudentId] = [Students].[Students].[StudentReferenceNumber];");

            migrationBuilder.DropColumn(
                name: "xStudentId",
                table: "SciencePracs_Attendance");

            migrationBuilder.CreateIndex(
                name: "IX_SciencePracs_Attendance_StudentId",
                table: "SciencePracs_Attendance",
                column: "StudentId");

            #endregion

            #region Reports_AcademicReports

            migrationBuilder.DropIndex(
                name: "IX_Reports_AcademicReports_StudentId",
                table: "Reports_AcademicReports");

            migrationBuilder.RenameColumn(
                name: "StudentId",
                table: "Reports_AcademicReports",
                newName: "xStudentId");

            migrationBuilder.AddColumn<Guid>(
                name: "StudentId",
                table: "Reports_AcademicReports",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.Sql(
                @"UPDATE [dbo].[Reports_AcademicReports]
                 SET [StudentId] = [Students].[Students].[Id]
                 FROM [Students].[Students]
                 INNER JOIN [dbo].[Reports_AcademicReports]
                 ON [dbo].[Reports_AcademicReports].[xStudentId] = [Students].[Students].[StudentReferenceNumber];");

            migrationBuilder.DropColumn(
                name: "xStudentId",
                table: "Reports_AcademicReports");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_AcademicReports_StudentId",
                table: "Reports_AcademicReports",
                column: "StudentId");

            #endregion

            #region MSTeamOperations

            migrationBuilder.DropForeignKey(
                name: "FK_MSTeamOperations_Students_StudentEnrolledMSTeamOperation_StudentId",
                table: "MSTeamOperations");

            migrationBuilder.DropForeignKey(
                name: "FK_MSTeamOperations_Students_StudentId1",
                table: "MSTeamOperations");

            migrationBuilder.DropForeignKey(
                name: "FK_MSTeamOperations_Students_StudentMSTeamOperation_StudentId",
                table: "MSTeamOperations");

            migrationBuilder.DropIndex(
                name: "IX_MSTeamOperations_StudentId1",
                table: "MSTeamOperations");

            migrationBuilder.DropIndex(
                name: "IX_MSTeamOperations_StudentMSTeamOperation_StudentId",
                table: "MSTeamOperations");

            migrationBuilder.DropIndex(
                name: "IX_MSTeamOperations_StudentEnrolledMSTeamOperation_StudentId",
                table: "MSTeamOperations");

            migrationBuilder.RenameColumn(
                name: "StudentMSTeamOperation_StudentId",
                table: "MSTeamOperations",
                newName: "xStudentId");

            migrationBuilder.AddColumn<Guid>(
                name: "StudentMSTeamOperation_StudentId",
                table: "MSTeamOperations",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql(
                @"UPDATE [dbo].[MSTeamOperations]
                 SET [StudentMSTeamOperation_StudentId] = [Students].[Students].[Id]
                 FROM [Students].[Students]
                 INNER JOIN [dbo].[MSTeamOperations]
                 ON [dbo].[MSTeamOperations].[xStudentId] = [Students].[Students].[StudentReferenceNumber];");

            migrationBuilder.DropColumn(
                name: "xStudentId",
                table: "MSTeamOperations");

            migrationBuilder.RenameColumn(
                name: "StudentId",
                table: "MSTeamOperations",
                newName: "xStudentId");

            migrationBuilder.AddColumn<Guid>(
                name: "StudentId",
                table: "MSTeamOperations",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql(
                @"UPDATE [dbo].[MSTeamOperations]
                 SET [StudentId] = [Students].[Students].[Id]
                 FROM [Students].[Students]
                 INNER JOIN [dbo].[MSTeamOperations]
                 ON [dbo].[MSTeamOperations].[xStudentId] = [Students].[Students].[StudentReferenceNumber];");

            migrationBuilder.DropColumn(
                name: "xStudentId",
                table: "MSTeamOperations");

            migrationBuilder.RenameColumn(
                name: "StudentEnrolledMSTeamOperation_StudentId",
                table: "MSTeamOperations",
                newName: "xStudentId");

            migrationBuilder.AddColumn<Guid>(
                name: "StudentEnrolledMSTeamOperation_StudentId",
                table: "MSTeamOperations",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql(
                @"UPDATE [dbo].[MSTeamOperations]
                 SET [StudentEnrolledMSTeamOperation_StudentId] = [Students].[Students].[Id]
                 FROM [Students].[Students]
                 INNER JOIN [dbo].[MSTeamOperations]
                 ON [dbo].[MSTeamOperations].[xStudentId] = [Students].[Students].[StudentReferenceNumber];");

            migrationBuilder.DropColumn(
                name: "xStudentId",
                table: "MSTeamOperations");

            migrationBuilder.CreateIndex(
                name: "IX_MSTeamOperations_StudentMSTeamOperation_StudentId",
                table: "MSTeamOperations",
                column: "StudentMSTeamOperation_StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_MSTeamOperations_StudentEnrolledMSTeamOperation_StudentId",
                table: "MSTeamOperations",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_MSTeamOperations_StudentId1",
                table: "MSTeamOperations",
                column: "StudentId");

            migrationBuilder.AddForeignKey(
                name: "FK_MSTeamOperations_Students_StudentMSTeamOperation_StudentId",
                table: "MSTeamOperations",
                column: "StudentMSTeamOperation_StudentId",
                principalSchema: "Students",
                principalTable: "Students",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MSTeamOperations_Students_StudentEnrolledMSTeamOperation_StudentId",
                table: "MSTeamOperations",
                column: "StudentEnrolledMSTeamOperation_StudentId",
                principalSchema: "Students",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MSTeamOperations_Students_StudentId1",
                table: "MSTeamOperations",
                column: "StudentId",
                principalSchema: "Students",
                principalTable: "Students",
                principalColumn: "Id");

            #endregion

            #region GroupTutorials_RollStudent

            migrationBuilder.DropPrimaryKey(
                name: "PK_GroupTutorials_RollStudent",
                table: "GroupTutorials_RollStudent");

            migrationBuilder.DropIndex(
                name: "IX_GroupTutorials_RollStudent_StudentId",
                table: "GroupTutorials_RollStudent");

            migrationBuilder.RenameColumn(
                name: "StudentId",
                table: "GroupTutorials_RollStudent",
                newName: "xStudentId");

            migrationBuilder.AddColumn<Guid>(
                name: "StudentId",
                table: "GroupTutorials_RollStudent",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.Sql(
                @"UPDATE [dbo].[GroupTutorials_RollStudent]
                 SET [StudentId] = [Students].[Students].[Id]
                 FROM [Students].[Students]
                 INNER JOIN [dbo].[GroupTutorials_RollStudent]
                 ON [dbo].[GroupTutorials_RollStudent].[xStudentId] = [Students].[Students].[StudentReferenceNumber];");

            migrationBuilder.DropColumn(
                name: "xStudentId",
                table: "GroupTutorials_RollStudent");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GroupTutorials_RollStudent",
                table: "GroupTutorials_RollStudent",
                columns: new[] { "StudentId", "TutorialRollId" });

            migrationBuilder.CreateIndex(
                name: "IX_GroupTutorials_RollStudent_StudentId",
                table: "GroupTutorials_RollStudent",
                column: "StudentId");

            #endregion

            #region GroupTutorials_Enrolment

            migrationBuilder.DropIndex(
                name: "IX_GroupTutorials_Enrolment_StudentId",
                table: "GroupTutorials_Enrolment");

            migrationBuilder.RenameColumn(
                name: "StudentId",
                table: "GroupTutorials_Enrolment",
                newName: "xStudentId");

            migrationBuilder.AddColumn<Guid>(
                name: "StudentId",
                table: "GroupTutorials_Enrolment",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.Sql(
                @"UPDATE [dbo].[GroupTutorials_Enrolment]
                 SET [StudentId] = [Students].[Students].[Id]
                 FROM [Students].[Students]
                 INNER JOIN [dbo].[GroupTutorials_Enrolment]
                 ON [dbo].[GroupTutorials_Enrolment].[xStudentId] = [Students].[Students].[StudentReferenceNumber];");

            migrationBuilder.DropColumn(
                name: "xStudentId",
                table: "GroupTutorials_Enrolment");

            migrationBuilder.CreateIndex(
                name: "IX_GroupTutorials_Enrolment_StudentId",
                table: "GroupTutorials_Enrolment",
                column: "StudentId");

            #endregion

            #region Families_StudentMemberships

            migrationBuilder.DropPrimaryKey(
                name: "PK_Families_StudentMemberships",
                table: "Families_StudentMemberships");

            migrationBuilder.DropIndex(
                name: "IX_Families_StudentMemberships_StudentId",
                table: "Families_StudentMemberships");

            migrationBuilder.RenameColumn(
                name: "StudentId",
                table: "Families_StudentMemberships",
                newName: "xStudentId");

            migrationBuilder.AddColumn<Guid>(
                name: "StudentId",
                table: "Families_StudentMemberships",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.Sql(
                @"UPDATE [dbo].[Families_StudentMemberships]
                 SET [StudentId] = [Students].[Students].[Id]
                 FROM [Students].[Students]
                 INNER JOIN [dbo].[Families_StudentMemberships]
                 ON [dbo].[Families_StudentMemberships].[xStudentId] = [Students].[Students].[StudentReferenceNumber];");

            migrationBuilder.DropColumn(
                name: "xStudentId",
                table: "Families_StudentMemberships");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Families_StudentMemberships",
                table: "Families_StudentMemberships",
                columns: new[] { "StudentId", "FamilyId" });

            migrationBuilder.CreateIndex(
                name: "IX_Families_StudentMemberships_StudentId",
                table: "Families_StudentMemberships",
                column: "StudentId");

            #endregion

            #region Enrolments

            migrationBuilder.DropIndex(
                name: "IX_Enrolments_StudentId",
                table: "Enrolments");

            migrationBuilder.RenameColumn(
                name: "StudentId",
                table: "Enrolments",
                newName: "xStudentId");

            migrationBuilder.AddColumn<Guid>(
                name: "StudentId",
                table: "Enrolments",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.Sql(
                @"UPDATE [dbo].[Enrolments]
                 SET [StudentId] = [Students].[Students].[Id]
                 FROM [Students].[Students]
                 INNER JOIN [dbo].[Enrolments]
                 ON [dbo].[Enrolments].[xStudentId] = [Students].[Students].[StudentReferenceNumber];");

            migrationBuilder.DropColumn(
                name: "xStudentId",
                table: "Enrolments");

            migrationBuilder.CreateIndex(
                name: "IX_Enrolments_StudentId",
                table: "Enrolments",
                column: "StudentId");

            #endregion

            #region DeviceAllocations

            migrationBuilder.DropIndex(
                name: "IX_DeviceAllocations_StudentId",
                table: "DeviceAllocations");

            migrationBuilder.RenameColumn(
                name: "StudentId",
                table: "DeviceAllocations",
                newName: "xStudentId");

            migrationBuilder.AddColumn<Guid>(
                name: "StudentId",
                table: "DeviceAllocations",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.Sql(
                @"UPDATE [dbo].[DeviceAllocations]
                 SET [StudentId] = [Students].[Students].[Id]
                 FROM [Students].[Students]
                 INNER JOIN [dbo].[DeviceAllocations]
                 ON [dbo].[DeviceAllocations].[xStudentId] = [Students].[Students].[StudentReferenceNumber];");

            migrationBuilder.DropColumn(
                name: "xStudentId",
                table: "DeviceAllocations");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceAllocations_StudentId",
                table: "DeviceAllocations",
                column: "StudentId");

            #endregion

            #region Awards_StudentAwards

            migrationBuilder.DropIndex(
                name: "IX_Awards_StudentAwards_StudentId",
                table: "Awards_StudentAwards");

            migrationBuilder.RenameColumn(
                name: "StudentId",
                table: "Awards_StudentAwards",
                newName: "xStudentId");

            migrationBuilder.AddColumn<Guid>(
                name: "StudentId",
                table: "Awards_StudentAwards",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.Sql(
                @"UPDATE [dbo].[Awards_StudentAwards]
                 SET [StudentId] = [Students].[Students].[Id]
                 FROM [Students].[Students]
                 INNER JOIN [dbo].[Awards_StudentAwards]
                 ON [dbo].[Awards_StudentAwards].[xStudentId] = [Students].[Students].[StudentReferenceNumber];");

            migrationBuilder.DropColumn(
                name: "xStudentId",
                table: "Awards_StudentAwards");

            migrationBuilder.CreateIndex(
                name: "IX_Awards_StudentAwards_StudentId",
                table: "Awards_StudentAwards",
                column: "StudentId");

            #endregion

            #region Awards_Nominations

            migrationBuilder.DropIndex(
                name: "IX_Awards_Nominations_StudentId",
                table: "Awards_Nominations");

            migrationBuilder.RenameColumn(
                name: "StudentId",
                table: "Awards_Nominations",
                newName: "xStudentId");

            migrationBuilder.AddColumn<Guid>(
                name: "StudentId",
                table: "Awards_Nominations",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.Sql(
                @"UPDATE [dbo].[Awards_Nominations]
                 SET [StudentId] = [Students].[Students].[Id]
                 FROM [Students].[Students]
                 INNER JOIN [dbo].[Awards_Nominations]
                 ON [dbo].[Awards_Nominations].[xStudentId] = [Students].[Students].[StudentReferenceNumber];");

            migrationBuilder.DropColumn(
                name: "xStudentId",
                table: "Awards_Nominations");

            migrationBuilder.CreateIndex(
                name: "IX_Awards_Nominations_StudentId",
                table: "Awards_Nominations",
                column: "StudentId");

            #endregion

            #region CanvasOperations

            migrationBuilder.AddColumn<string>(
                name: "UpdateUserEmailCanvasOperation_PortalUsername",
                table: "CanvasOperations",
                type: "nvarchar(max)",
                nullable: true);

            #endregion CanvasOperations

            #region Attendance_Values

            migrationBuilder.DropIndex(
                name: "IX_Attendance_Values_StudentId",
                table: "Attendance_Values");

            migrationBuilder.RenameColumn(
                name: "StudentId",
                table: "Attendance_Values",
                newName: "xStudentId");

            migrationBuilder.AddColumn<Guid>(
                name: "StudentId",
                table: "Attendance_Values",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.Sql(
                @"UPDATE [dbo].[Attendance_Values]
                 SET [StudentId] = [Students].[Students].[Id]
                 FROM [Students].[Students]
                 INNER JOIN [dbo].[Attendance_Values]
                 ON [dbo].[Attendance_Values].[xStudentId] = [Students].[Students].[StudentReferenceNumber];");

            migrationBuilder.DropColumn(
                name: "xStudentId",
                table: "Attendance_Values");

            migrationBuilder.CreateIndex(
                name: "IX_Attendance_Values_StudentId",
                table: "Attendance_Values",
                column: "StudentId");

            #endregion

            #region Assignments_Submissions

            migrationBuilder.DropIndex(
                name: "IX_Assignments_Submissions_StudentId",
                table: "Assignments_Submissions");

            migrationBuilder.RenameColumn(
                name: "StudentId",
                table: "Assignments_Submissions",
                newName: "xStudentId");

            migrationBuilder.AddColumn<Guid>(
                name: "StudentId",
                table: "Assignments_Submissions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.Sql(
                @"UPDATE [dbo].[Assignments_Submissions]
                 SET [StudentId] = [Students].[Students].[Id]
                 FROM [Students].[Students]
                 INNER JOIN [dbo].[Assignments_Submissions]
                 ON [dbo].[Assignments_Submissions].[xStudentId] = [Students].[Students].[StudentReferenceNumber];");

            migrationBuilder.DropColumn(
                name: "xStudentId",
                table: "Assignments_Submissions");

            migrationBuilder.CreateIndex(
                name: "IX_Assignments_Submissions_StudentId",
                table: "Assignments_Submissions",
                column: "StudentId");

            #endregion

            #region AspNetUsers

            migrationBuilder.AlterColumn<Guid>(
                name: "StudentId",
                table: "AspNetUsers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            #endregion

            #region Absences_Absences

            migrationBuilder.DropIndex(
                name: "IX_Absences_Absences_StudentId",
                table: "Absences_Absences");

            migrationBuilder.RenameColumn(
                name: "StudentId",
                table: "Absences_Absences",
                newName: "xStudentId");

            migrationBuilder.AddColumn<Guid>(
                name: "StudentId",
                table: "Absences_Absences",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.Sql(
                @"UPDATE [dbo].[Absences_Absences]
                 SET [StudentId] = [Students].[Students].[Id]
                 FROM [Students].[Students]
                 INNER JOIN [dbo].[Absences_Absences]
                 ON [dbo].[Absences_Absences].[xStudentId] = [Students].[Students].[StudentReferenceNumber];");

            migrationBuilder.DropColumn(
                name: "xStudentId",
                table: "Absences_Absences");

            migrationBuilder.CreateIndex(
                name: "IX_Absences_Absences_StudentId",
                table: "Absences_Absences",
                column: "StudentId");

            #endregion

            #region Assets.Allocations

            migrationBuilder.Sql(
                @"UPDATE [Assets].[Allocations]
                 SET [UserId] = [Students].[Students].[Id]
                 FROM [Students].[Students]
                 INNER JOIN [Assets].[Allocations]
                 ON [Assets].[Allocations].[UserId] = [Students].[Students].[StudentReferenceNumber];");

            #endregion

            migrationBuilder.CreateIndex(
                name: "IX_Students_StudentReferenceNumber",
                schema: "Students",
                table: "Students",
                column: "StudentReferenceNumber",
                unique: true,
                filter: "[StudentReferenceNumber] IS NOT NULL");

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
                name: "FK_Assignments_Submissions_Students_StudentId",
                table: "Assignments_Submissions",
                column: "StudentId",
                principalSchema: "Students",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Attendance_Values_Students_StudentId",
                table: "Attendance_Values",
                column: "StudentId",
                principalSchema: "Students",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Awards_Nominations_Students_StudentId",
                table: "Awards_Nominations",
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
                name: "FK_Families_StudentMemberships_Students_StudentId",
                table: "Families_StudentMemberships",
                column: "StudentId",
                principalSchema: "Students",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GroupTutorials_Enrolment_Students_StudentId",
                table: "GroupTutorials_Enrolment",
                column: "StudentId",
                principalSchema: "Students",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GroupTutorials_RollStudent_Students_StudentId",
                table: "GroupTutorials_RollStudent",
                column: "StudentId",
                principalSchema: "Students",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_AcademicReports_Students_StudentId",
                table: "Reports_AcademicReports",
                column: "StudentId",
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

            //migrationBuilder.AddForeignKey(
            //    name: "FK_Students_AbsenceConfiguration_Students_StudentId",
            //    table: "Students_AbsenceConfiguration",
            //    column: "StudentId",
            //    principalSchema: "Students",
            //    principalTable: "Students",
            //    principalColumn: "Id",
            //    onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ThirdParty_Consents_Students_StudentId",
                table: "ThirdParty_Consents",
                column: "StudentId",
                principalSchema: "Students",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ThirdParty_Transactions_Students_StudentId",
                table: "ThirdParty_Transactions",
                column: "StudentId",
                principalSchema: "Students",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.CreateIndex(
                name: "IX_AbsenceConfigurations_StudentId_AbsenceType_CalendarYear",
                schema: "Students",
                table: "AbsenceConfigurations",
                columns: new[] { "StudentId", "AbsenceType", "CalendarYear" });


        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Absences_Absences_Students_StudentId",
                table: "Absences_Absences");

            migrationBuilder.DropForeignKey(
                name: "FK_Assignments_Submissions_Students_StudentId",
                table: "Assignments_Submissions");

            migrationBuilder.DropForeignKey(
                name: "FK_Attendance_Values_Students_StudentId",
                table: "Attendance_Values");

            migrationBuilder.DropForeignKey(
                name: "FK_Awards_Nominations_Students_StudentId",
                table: "Awards_Nominations");

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
                name: "FK_Families_StudentMemberships_Students_StudentId",
                table: "Families_StudentMemberships");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupTutorials_Enrolment_Students_StudentId",
                table: "GroupTutorials_Enrolment");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupTutorials_RollStudent_Students_StudentId",
                table: "GroupTutorials_RollStudent");

            migrationBuilder.DropForeignKey(
                name: "FK_MSTeamOperations_Students_StudentEnrolledMSTeamOperation_StudentId",
                table: "MSTeamOperations");

            migrationBuilder.DropForeignKey(
                name: "FK_MSTeamOperations_Students_StudentId1",
                table: "MSTeamOperations");

            migrationBuilder.DropForeignKey(
                name: "FK_MSTeamOperations_Students_StudentMSTeamOperation_StudentId",
                table: "MSTeamOperations");

            migrationBuilder.DropForeignKey(
                name: "FK_Reports_AcademicReports_Students_StudentId",
                table: "Reports_AcademicReports");

            migrationBuilder.DropForeignKey(
                name: "FK_SciencePracs_Attendance_Students_StudentId",
                table: "SciencePracs_Attendance");

            migrationBuilder.DropForeignKey(
                name: "FK_Students_AbsenceConfiguration_Students_StudentId",
                table: "Students_AbsenceConfiguration");

            migrationBuilder.DropForeignKey(
                name: "FK_ThirdParty_Consents_Students_StudentId",
                table: "ThirdParty_Consents");

            migrationBuilder.DropForeignKey(
                name: "FK_ThirdParty_Transactions_Students_StudentId",
                table: "ThirdParty_Transactions");

            migrationBuilder.DropTable(
                name: "AwardTallies",
                schema: "Students");

            migrationBuilder.DropTable(
                name: "SchoolEnrolments",
                schema: "Students");

            migrationBuilder.DropTable(
                name: "SystemLinks",
                schema: "Students");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Students",
                schema: "Students",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Students_StudentReferenceNumber",
                schema: "Students",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "UpdateUserEmailCanvasOperation_PortalUsername",
                table: "CanvasOperations");

            migrationBuilder.DropColumn(
                name: "Id",
                schema: "Students",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                schema: "Students",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "Students",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                schema: "Students",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                schema: "Students",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "EmailAddress",
                schema: "Students",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "ModifiedAt",
                schema: "Students",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "StudentReferenceNumber",
                schema: "Students",
                table: "Students");

            migrationBuilder.RenameTable(
                name: "Students",
                schema: "Students",
                newName: "Students");

            migrationBuilder.RenameColumn(
                name: "Name_LastName",
                table: "Students",
                newName: "LastName");

            migrationBuilder.RenameColumn(
                name: "Name_FirstName",
                table: "Students",
                newName: "FirstName");

            migrationBuilder.RenameColumn(
                name: "PreferredGender",
                table: "Students",
                newName: "SentralStudentId");

            migrationBuilder.RenameColumn(
                name: "Name_PreferredName",
                table: "Students",
                newName: "PortalUsername");

            migrationBuilder.RenameColumn(
                name: "ModifiedBy",
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
                name: "LastName",
                table: "Students",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "FirstName",
                table: "Students",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "StudentId",
                table: "Students",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

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

            migrationBuilder.AddPrimaryKey(
                name: "PK_Students",
                table: "Students",
                column: "StudentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Absences_Absences_Students_StudentId",
                table: "Absences_Absences",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "StudentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Assignments_Submissions_Students_StudentId",
                table: "Assignments_Submissions",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "StudentId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Attendance_Values_Students_StudentId",
                table: "Attendance_Values",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "StudentId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Awards_Nominations_Students_StudentId",
                table: "Awards_Nominations",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "StudentId",
                onDelete: ReferentialAction.Cascade);

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
                name: "FK_Families_StudentMemberships_Students_StudentId",
                table: "Families_StudentMemberships",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "StudentId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GroupTutorials_Enrolment_Students_StudentId",
                table: "GroupTutorials_Enrolment",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "StudentId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GroupTutorials_RollStudent_Students_StudentId",
                table: "GroupTutorials_RollStudent",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "StudentId",
                onDelete: ReferentialAction.Restrict);

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
                name: "FK_MSTeamOperations_Students_StudentMSTeamOperation_StudentId",
                table: "MSTeamOperations",
                column: "StudentMSTeamOperation_StudentId",
                principalTable: "Students",
                principalColumn: "StudentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_AcademicReports_Students_StudentId",
                table: "Reports_AcademicReports",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "StudentId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SciencePracs_Attendance_Students_StudentId",
                table: "SciencePracs_Attendance",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "StudentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Students_AbsenceConfiguration_Students_StudentId",
                table: "Students_AbsenceConfiguration",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "StudentId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ThirdParty_Consents_Students_StudentId",
                table: "ThirdParty_Consents",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "StudentId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ThirdParty_Transactions_Students_StudentId",
                table: "ThirdParty_Transactions",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "StudentId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
