using System;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    /// <inheritdoc />
    public partial class ConvertStaffToNewAggregate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Staff");

            migrationBuilder.CreateTable(
                name: "Members",
                schema: "Staff",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PreferredName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmailAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Gender = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsShared = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateDeleted = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Members", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SchoolAssignments",
                schema: "Staff",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StaffId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SchoolCode = table.Column<string>(type: "nvarchar(4)", nullable: false),
                    SchoolName = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_SchoolAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SchoolAssignments_Members_StaffId",
                        column: x => x.StaffId,
                        principalSchema: "Staff",
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SchoolAssignments_Schools_SchoolCode",
                        column: x => x.SchoolCode,
                        principalTable: "Schools",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateTable(
                name: "SystemLinks",
                schema: "Staff",
                columns: table => new
                {
                    System = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    StaffId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemLinks", x => new { x.StaffId, x.System });
                    table.ForeignKey(
                        name: "FK_SystemLinks_Members_StaffId",
                        column: x => x.StaffId,
                        principalSchema: "Staff",
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "Staff",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql(
                @"UPDATE [dbo].[Staff]
                    SET [Id] = NEWID()
                    WHERE 1 = 1;");

            migrationBuilder.Sql(
                @"INSERT INTO [Staff].[Members]
                    ([Id],
                     [EmployeeId],
                     [FirstName],
                     [PreferredName],
                     [LastName],
                     [EmailAddress],
                     [Gender],
                     [IsShared],
                     [CreatedBy],
                     [CreatedAt],
                     [ModifiedBy],
                     [ModifiedAt],
                     [IsDeleted],
                     [DeletedBy],
                     [DeletedAt])
                    SELECT
                     Id,
                     StaffId,
                     FirstName,
                     FirstName,
                     LastName,
                     LTRIM(CONCAT(PortalUsername, '@det.nsw.edu.au')),
                     'Male',
                     IsShared,
                     'System Migration',
                      CASE
                        WHEN DateEntered is null THEN CAST('0001-01-01' as DateTime2)
                        ELSE DateEntered
                      End,
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
                    FROM [dbo].[Staff]");

            migrationBuilder.Sql(
                @"INSERT INTO [Staff].[SchoolAssignments]
                    ([Id],
                     [StaffId],
                     [SchoolCode],
                     [SchoolName],
                     [StartDate],
                     [EndDate],
                     [CreatedBy],
                     [CreatedAt],
                     [ModifiedBy],
                     [ModifiedAt],
                     [IsDeleted],
                     [DeletedBy],
                     [DeletedAt])
                    SELECT
                     NEWID(),
                     Id,
                     SchoolCode,
                     Schools.Name,
                     CASE
                        WHEN DateEntered is null THEN DateDeleted
                        ELSE DateEntered
                     End,
                     NULL,
                     'System Migration',
                     CASE
                        WHEN DateEntered is null THEN CAST('0001-01-01' as DateTime2)
                        ELSE DateEntered
                     END,
                     NULL,
                     CAST('0001-01-01' as DateTime2),
                     '0',
                     NULL,
                     CAST('0001-01-01' as DateTime2)
                    FROM [dbo].[Staff]
                    JOIN [dbo].[Schools] on Staff.SchoolCode = Schools.Code");

            migrationBuilder.DropForeignKey(
                name: "FK_Assignees_Staff_StaffId",
                schema: "Training",
                table: "Assignees");

            migrationBuilder.DropForeignKey(
                name: "FK_Awards_StudentAwards_Staff_TeacherId",
                table: "Awards_StudentAwards");

            migrationBuilder.DropForeignKey(
                name: "FK_Completions_Staff_StaffId",
                schema: "Training",
                table: "Completions");

            migrationBuilder.DropForeignKey(
                name: "FK_Faculties_Memberships_Staff_StaffId",
                table: "Faculties_Memberships");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupTutorials_Roll_Staff_StaffId",
                table: "GroupTutorials_Roll");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupTutorials_Teachers_Staff_StaffId",
                table: "GroupTutorials_Teachers");

            migrationBuilder.DropForeignKey(
                name: "FK_MSTeamOperations_Staff_StaffId",
                table: "MSTeamOperations");

            migrationBuilder.DropForeignKey(
                name: "FK_MSTeamOperations_Staff_TeacherEmployedMSTeamOperation_StaffId",
                table: "MSTeamOperations");

            migrationBuilder.DropForeignKey(
                name: "FK_MSTeamOperations_Staff_TeacherMSTeamOperation_StaffId",
                table: "MSTeamOperations");

            migrationBuilder.DropForeignKey(
                name: "FK_Offerings_Teachers_Staff_StaffId",
                table: "Offerings_Teachers");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkFlows_Actions_Staff_AssignedToId",
                table: "WorkFlows_Actions");

            #region WorkFlows_CaseDetails

            migrationBuilder.RenameColumn(
                name: "StaffId",
                table: "WorkFlows_CaseDetails",
                newName: "xStaffId");

            migrationBuilder.AddColumn<Guid>(
                name: "StaffId",
                table: "WorkFlows_CaseDetails",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql(
                @"UPDATE [dbo].[WorkFlows_CaseDetails]
                    SET [StaffId] = [Staff].[Members].[Id]
                    FROM [Staff].[Members]
                    INNER JOIN [dbo].[WorkFlows_CaseDetails]
                    ON [dbo].[WorkFlows_CaseDetails].[xStaffId] = [Staff].[Members].[EmployeeId];");

            migrationBuilder.DropColumn(
                name: "xStaffId",
                table: "WorkFlows_CaseDetails");

            migrationBuilder.RenameColumn(
                name: "CreatedById",
                table: "WorkFlows_CaseDetails",
                newName: "xCreatedById");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedById",
                table: "WorkFlows_CaseDetails",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql(
                @"UPDATE [dbo].[WorkFlows_CaseDetails]
                    SET [CreatedById] = [Staff].[Members].[Id]
                    FROM [Staff].[Members]
                    INNER JOIN [dbo].[WorkFlows_CaseDetails]
                    ON [dbo].[WorkFlows_CaseDetails].[xCreatedById] = [Staff].[Members].[EmployeeId];");

            migrationBuilder.DropColumn(
                name: "xCreatedById",
                table: "WorkFlows_CaseDetails");

            #endregion

            #region WorkFlows_Actions

            migrationBuilder.DropIndex(
                name: "IX_WorkFlows_Actions_AssignedToId",
                table: "WorkFlows_Actions");

            migrationBuilder.RenameColumn(
                name: "AssignedToId",
                table: "WorkFlows_Actions",
                newName: "xAssignedToId");

            migrationBuilder.AddColumn<Guid>(
                name: "AssignedToId",
                table: "WorkFlows_Actions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql(
                @"UPDATE [dbo].[WorkFlows_Actions]
                    SET [AssignedToId] = [Staff].[Members].[Id]
                    FROM [Staff].[Members]
                    INNER JOIN [dbo].[WorkFlows_Actions]
                    ON [dbo].[WorkFlows_Actions].[xAssignedToId] = [Staff].[Members].[EmployeeId];");
            
            migrationBuilder.DropColumn(
                name: "xAssignedToId",
                table: "WorkFlows_Actions");

            migrationBuilder.CreateIndex(
                name: "IX_WorkFlows_Actions_AssignedToId",
                table: "WorkFlows_Actions",
                column: "AssignedToId");

            #endregion

            #region Offerings_Teachers

            migrationBuilder.DropIndex(
                name: "IX_Offerings_Teachers_StaffId",
                table: "Offerings_Teachers");

            migrationBuilder.RenameColumn(
                name: "StaffId",
                table: "Offerings_Teachers",
                newName: "xStaffId");

            migrationBuilder.AddColumn<Guid>(
                name: "StaffId",
                table: "Offerings_Teachers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.Sql(
                @"UPDATE [dbo].[Offerings_Teachers]
                    SET [StaffId] = [Staff].[Members].[Id]
                    FROM [Staff].[Members]
                    INNER JOIN [dbo].[Offerings_Teachers]
                    ON [dbo].[Offerings_Teachers].[xStaffId] = [Staff].[Members].[EmployeeId];");

            migrationBuilder.DropColumn(
                name: "xStaffId",
                table: "Offerings_Teachers");

            migrationBuilder.CreateIndex(
                name: "IX_Offerings_Teachers_StaffId",
                table: "Offerings_Teachers",
                column: "StaffId");

            #endregion

            #region MSTeamOperations

            migrationBuilder.DropIndex(
                name: "IX_MSTeamOperations_TeacherMSTeamOperation_StaffId",
                table: "MSTeamOperations");

            migrationBuilder.DropIndex(
                name: "IX_MSTeamOperations_TeacherEmployedMSTeamOperation_StaffId",
                table: "MSTeamOperations");

            migrationBuilder.DropIndex(
                name: "IX_MSTeamOperations_StaffId",
                table: "MSTeamOperations");

            migrationBuilder.DropColumn(
                name: "TeacherEmployedMSTeamOperation_StaffId",
                table: "MSTeamOperations");

            migrationBuilder.RenameColumn(
                name: "TeacherMSTeamOperation_StaffId",
                table: "MSTeamOperations",
                newName: "xTeacherMSTeamOperation_StaffId");

            migrationBuilder.AddColumn<Guid>(
                name: "TeacherMSTeamOperation_StaffId",
                table: "MSTeamOperations",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql(
                @"UPDATE [dbo].[MSTeamOperations]
                    SET [TeacherMSTeamOperation_StaffId] = [Staff].[Members].[Id]
                    FROM [Staff].[Members]
                    INNER JOIN [dbo].[MSTeamOperations]
                    ON [dbo].[MSTeamOperations].[xTeacherMSTeamOperation_StaffId] = [Staff].[Members].[EmployeeId];");

            migrationBuilder.DropColumn(
                name: "xTeacherMSTeamOperation_StaffId",
                table: "MSTeamOperations");

            migrationBuilder.RenameColumn(
                name: "StaffId",
                table: "MSTeamOperations",
                newName: "xStaffId");

            migrationBuilder.AddColumn<Guid>(
                name: "StaffId",
                table: "MSTeamOperations",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql(
                @"UPDATE [dbo].[MSTeamOperations]
                    SET [StaffId] = [Staff].[Members].[Id]
                    FROM [Staff].[Members]
                    INNER JOIN [dbo].[MSTeamOperations]
                    ON [dbo].[MSTeamOperations].[xStaffId] = [Staff].[Members].[EmployeeId];");

            migrationBuilder.DropColumn(
                name: "xStaffId",
                table: "MSTeamOperations");

            migrationBuilder.AddColumn<Guid>(
                name: "StaffId1",
                table: "MSTeamOperations",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MSTeamOperations_TeacherMSTeamOperation_StaffId",
                table: "MSTeamOperations",
                column: "TeacherMSTeamOperation_StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_MSTeamOperations_StaffId1",
                table: "MSTeamOperations",
                column: "StaffId1");

            migrationBuilder.CreateIndex(
                name: "IX_MSTeamOperations_StaffId",
                table: "MSTeamOperations",
                column: "StaffId");

            #endregion

            #region GroupTutorials_Teachers

            migrationBuilder.DropIndex(
                name: "IX_GroupTutorials_Teachers_StaffId",
                table: "GroupTutorials_Teachers");

            migrationBuilder.RenameColumn(
                name: "StaffId",
                table: "GroupTutorials_Teachers",
                newName: "xStaffId");

            migrationBuilder.AddColumn<Guid>(
                name: "StaffId",
                table: "GroupTutorials_Teachers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.Sql(
                @"UPDATE [dbo].[GroupTutorials_Teachers]
                    SET [StaffId] = [Staff].[Members].[Id]
                    FROM [Staff].[Members]
                    INNER JOIN [dbo].[GroupTutorials_Teachers]
                    ON [dbo].[GroupTutorials_Teachers].[xStaffId] = [Staff].[Members].[EmployeeId];");

            migrationBuilder.DropColumn(
                name: "xStaffId",
                table: "GroupTutorials_Teachers");

            migrationBuilder.CreateIndex(
                name: "IX_GroupTutorials_Teachers_StaffId",
                table: "GroupTutorials_Teachers",
                column: "StaffId");

            #endregion

            #region GroupTutorials_Roll

            migrationBuilder.DropIndex(
                name: "IX_GroupTutorials_Roll_StaffId",
                table: "GroupTutorials_Roll");

            migrationBuilder.RenameColumn(
                name: "StaffId",
                table: "GroupTutorials_Roll",
                newName: "xStaffId");

            migrationBuilder.AddColumn<Guid>(
                name: "StaffId",
                table: "GroupTutorials_Roll",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.Sql(
                @"UPDATE [dbo].[GroupTutorials_Roll]
                    SET [StaffId] = [Staff].[Members].[Id]
                    FROM [Staff].[Members]
                    INNER JOIN [dbo].[GroupTutorials_Roll]
                    ON [dbo].[GroupTutorials_Roll].[xStaffId] = [Staff].[Members].[EmployeeId];");

            migrationBuilder.DropColumn(
                name: "xStaffId",
                table: "GroupTutorials_Roll");

            migrationBuilder.CreateIndex(
                name: "IX_GroupTutorials_Roll_StaffId",
                table: "GroupTutorials_Roll",
                column: "StaffId");

            #endregion

            #region Faculties_Memberships

            migrationBuilder.DropIndex(
                name: "IX_Faculties_Memberships_StaffId",
                table: "Faculties_Memberships");

            migrationBuilder.DropIndex(
                name: "IX_FacultyMembership_StaffId",
                table: "Faculties_Memberships");

            migrationBuilder.RenameColumn(
                name: "StaffId",
                table: "Faculties_Memberships",
                newName: "xStaffId");

            migrationBuilder.AddColumn<Guid>(
                name: "StaffId",
                table: "Faculties_Memberships",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.Sql(
                @"UPDATE [dbo].[Faculties_Memberships]
                    SET [StaffId] = [Staff].[Members].[Id]
                    FROM [Staff].[Members]
                    INNER JOIN [dbo].[Faculties_Memberships]
                    ON [dbo].[Faculties_Memberships].[xStaffId] = [Staff].[Members].[EmployeeId];");

            migrationBuilder.DropColumn(
                name: "xStaffId",
                table: "Faculties_Memberships");

            migrationBuilder.AddColumn<Guid>(
                name: "StaffMemberId",
                table: "Faculties_Memberships",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Faculties_Memberships_StaffMemberId",
                table: "Faculties_Memberships",
                column: "StaffMemberId");

            #endregion

            #region Training.Completions

            migrationBuilder.DropIndex(
                name: "IX_Training_Modules_Completions_StaffId",
                schema: "Training",
                table: "Completions");

            migrationBuilder.DropIndex(
                name: "IX_Completions_StaffId",
                schema: "Training",
                table: "Completions");

            migrationBuilder.RenameColumn(
                name: "StaffId",
                schema: "Training",
                table: "Completions",
                newName: "xStaffId");

            migrationBuilder.AddColumn<Guid>(
                name: "StaffId",
                schema: "Training",
                table: "Completions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.Sql(
                @"UPDATE [Training].[Completions]
                    SET [StaffId] = [Staff].[Members].[Id]
                    FROM [Staff].[Members]
                    INNER JOIN [Training].[Completions]
                    ON [Training].[Completions].[xStaffId] = [Staff].[Members].[EmployeeId];");

            migrationBuilder.DropColumn(
                name: "xStaffId",
                schema: "Training",
                table: "Completions");

            migrationBuilder.CreateIndex(
                name: "IX_Training_Modules_Completions_StaffId",
                schema: "Training",
                table: "Completions",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_Completions_StaffId",
                schema: "Training",
                table: "Completions",
                column: "StaffId");

            #endregion

            #region Training.Assignees

            migrationBuilder.DropPrimaryKey(
                name: "PK_Assignees",
                schema: "Training",
                table: "Assignees");

            migrationBuilder.DropIndex(
                name: "IX_Assignees_StaffId",
                schema: "Training",
                table: "Assignees");

            migrationBuilder.RenameColumn(
                name: "StaffId",
                schema: "Training",
                table: "Assignees",
                newName: "xStaffId");

            migrationBuilder.AddColumn<Guid>(
                name: "StaffId",
                schema: "Training",
                table: "Assignees",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.Sql(
                @"UPDATE [Training].[Assignees]
                    SET [StaffId] = [Staff].[Members].[Id]
                    FROM [Staff].[Members]
                    INNER JOIN [Training].[Assignees]
                    ON [Training].[Assignees].[xStaffId] = [Staff].[Members].[EmployeeId];");

            migrationBuilder.DropColumn(
                name: "xStaffId",
                schema: "Training",
                table: "Assignees");

            migrationBuilder.CreateIndex(
                name: "IX_Assignees_StaffId",
                schema: "Training",
                table: "Assignees",
                column: "StaffId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Assignees",
                schema: "Training",
                table: "Assignees",
                columns: ["ModuleId", "StaffId"]);

            #endregion

            #region Awards_StudentAwards

            migrationBuilder.DropIndex(
                name: "IX_Awards_StudentAwards_TeacherId",
                table: "Awards_StudentAwards");

            migrationBuilder.RenameColumn(
                name: "TeacherId",
                table: "Awards_StudentAwards",
                newName: "xTeacherId");

            migrationBuilder.AddColumn<Guid>(
                name: "TeacherId",
                table: "Awards_StudentAwards",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.Sql(
                @"UPDATE [dbo].[Awards_StudentAwards]
                    SET [TeacherId] = [Staff].[Members].[Id]
                    FROM [Staff].[Members]
                    INNER JOIN [dbo].[Awards_StudentAwards]
                    ON [dbo].[Awards_StudentAwards].[xTeacherId] = [Staff].[Members].[EmployeeId];");

            migrationBuilder.DropColumn(
                name: "xTeacherId",
                table: "Awards_StudentAwards");

            migrationBuilder.CreateIndex(
                name: "IX_Awards_StudentAwards_TeacherId",
                table: "Awards_StudentAwards",
                column: "TeacherId");

            #endregion

            #region AspNetUsers

            migrationBuilder.RenameColumn(
                name: "StaffId",
                table: "AspNetUsers",
                newName: "xStaffId");

            migrationBuilder.AddColumn<Guid>(
                name: "StaffId",
                table: "AspNetUsers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.Sql(
                @"UPDATE [dbo].[AspNetUsers]
                    SET [StaffId] = [Staff].[Members].[Id]
                    FROM [Staff].[Members]
                    INNER JOIN [dbo].[AspNetUsers]
                    ON [dbo].[AspNetUsers].[xStaffId] = [Staff].[Members].[EmployeeId];");

            migrationBuilder.DropColumn(
                name: "xStaffId",
                table: "AspNetUsers");

            #endregion


            migrationBuilder.CreateIndex(
                name: "IX_Members_EmployeeId",
                schema: "Staff",
                table: "Members",
                column: "EmployeeId",
                unique: true,
                filter: "[EmployeeId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolAssignments_SchoolCode",
                schema: "Staff",
                table: "SchoolAssignments",
                column: "SchoolCode");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolAssignments_StaffId",
                schema: "Staff",
                table: "SchoolAssignments",
                column: "StaffId");

            migrationBuilder.AddForeignKey(
                name: "FK_Assignees_Members_StaffId",
                schema: "Training",
                table: "Assignees",
                column: "StaffId",
                principalSchema: "Staff",
                principalTable: "Members",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Awards_StudentAwards_Members_TeacherId",
                table: "Awards_StudentAwards",
                column: "TeacherId",
                principalSchema: "Staff",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Completions_Members_StaffId",
                schema: "Training",
                table: "Completions",
                column: "StaffId",
                principalSchema: "Staff",
                principalTable: "Members",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Faculties_Memberships_Members_StaffMemberId",
                table: "Faculties_Memberships",
                column: "StaffMemberId",
                principalSchema: "Staff",
                principalTable: "Members",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupTutorials_Roll_Members_StaffId",
                table: "GroupTutorials_Roll",
                column: "StaffId",
                principalSchema: "Staff",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GroupTutorials_Teachers_Members_StaffId",
                table: "GroupTutorials_Teachers",
                column: "StaffId",
                principalSchema: "Staff",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MSTeamOperations_Members_StaffId",
                table: "MSTeamOperations",
                column: "StaffId",
                principalSchema: "Staff",
                principalTable: "Members",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MSTeamOperations_Members_StaffId1",
                table: "MSTeamOperations",
                column: "StaffId1",
                principalSchema: "Staff",
                principalTable: "Members",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MSTeamOperations_Members_TeacherMSTeamOperation_StaffId",
                table: "MSTeamOperations",
                column: "TeacherMSTeamOperation_StaffId",
                principalSchema: "Staff",
                principalTable: "Members",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Offerings_Teachers_Members_StaffId",
                table: "Offerings_Teachers",
                column: "StaffId",
                principalSchema: "Staff",
                principalTable: "Members",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkFlows_Actions_Members_AssignedToId",
                table: "WorkFlows_Actions",
                column: "AssignedToId",
                principalSchema: "Staff",
                principalTable: "Members",
                principalColumn: "Id");

            migrationBuilder.DropTable(
                name: "Staff");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assignees_Members_StaffId",
                schema: "Training",
                table: "Assignees");

            migrationBuilder.DropForeignKey(
                name: "FK_Awards_StudentAwards_Members_TeacherId",
                table: "Awards_StudentAwards");

            migrationBuilder.DropForeignKey(
                name: "FK_Completions_Members_StaffId",
                schema: "Training",
                table: "Completions");

            migrationBuilder.DropForeignKey(
                name: "FK_Faculties_Memberships_Members_StaffMemberId",
                table: "Faculties_Memberships");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupTutorials_Roll_Members_StaffId",
                table: "GroupTutorials_Roll");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupTutorials_Teachers_Members_StaffId",
                table: "GroupTutorials_Teachers");

            migrationBuilder.DropForeignKey(
                name: "FK_MSTeamOperations_Members_StaffId",
                table: "MSTeamOperations");

            migrationBuilder.DropForeignKey(
                name: "FK_MSTeamOperations_Members_StaffId1",
                table: "MSTeamOperations");

            migrationBuilder.DropForeignKey(
                name: "FK_MSTeamOperations_Members_TeacherMSTeamOperation_StaffId",
                table: "MSTeamOperations");

            migrationBuilder.DropForeignKey(
                name: "FK_Offerings_Teachers_Members_StaffId",
                table: "Offerings_Teachers");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkFlows_Actions_Members_AssignedToId",
                table: "WorkFlows_Actions");

            migrationBuilder.DropTable(
                name: "SchoolAssignments",
                schema: "Staff");

            migrationBuilder.DropTable(
                name: "SystemLinks",
                schema: "Staff");

            migrationBuilder.DropTable(
                name: "Members",
                schema: "Staff");

            migrationBuilder.DropIndex(
                name: "IX_MSTeamOperations_StaffId1",
                table: "MSTeamOperations");

            migrationBuilder.DropIndex(
                name: "IX_Faculties_Memberships_StaffMemberId",
                table: "Faculties_Memberships");

            migrationBuilder.DropColumn(
                name: "StaffId1",
                table: "MSTeamOperations");

            migrationBuilder.DropColumn(
                name: "StaffMemberId",
                table: "Faculties_Memberships");

            migrationBuilder.AlterColumn<string>(
                name: "StaffId",
                table: "WorkFlows_CaseDetails",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedById",
                table: "WorkFlows_CaseDetails",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AssignedToId",
                table: "WorkFlows_Actions",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "StaffId",
                table: "Offerings_Teachers",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "TeacherMSTeamOperation_StaffId",
                table: "MSTeamOperations",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "StaffId",
                table: "MSTeamOperations",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TeacherEmployedMSTeamOperation_StaffId",
                table: "MSTeamOperations",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "StaffId",
                table: "GroupTutorials_Teachers",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "StaffId",
                table: "GroupTutorials_Roll",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "StaffId",
                table: "Faculties_Memberships",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "StaffId",
                schema: "Training",
                table: "Completions",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "TeacherId",
                table: "Awards_StudentAwards",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "StaffId",
                schema: "Training",
                table: "Assignees",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "StaffId",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.CreateTable(
                name: "Staff",
                columns: table => new
                {
                    StaffId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SchoolCode = table.Column<string>(type: "nvarchar(4)", nullable: true),
                    AdobeConnectPrincipalId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateDeleted = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateEntered = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsShared = table.Column<bool>(type: "bit", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PortalUsername = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Staff", x => x.StaffId);
                    table.ForeignKey(
                        name: "FK_Staff_Schools_SchoolCode",
                        column: x => x.SchoolCode,
                        principalTable: "Schools",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateIndex(
                name: "IX_MSTeamOperations_TeacherEmployedMSTeamOperation_StaffId",
                table: "MSTeamOperations",
                column: "TeacherEmployedMSTeamOperation_StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_Faculties_Memberships_StaffId",
                table: "Faculties_Memberships",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_Staff_SchoolCode",
                table: "Staff",
                column: "SchoolCode");

            migrationBuilder.AddForeignKey(
                name: "FK_Assignees_Staff_StaffId",
                schema: "Training",
                table: "Assignees",
                column: "StaffId",
                principalTable: "Staff",
                principalColumn: "StaffId");

            migrationBuilder.AddForeignKey(
                name: "FK_Awards_StudentAwards_Staff_TeacherId",
                table: "Awards_StudentAwards",
                column: "TeacherId",
                principalTable: "Staff",
                principalColumn: "StaffId");

            migrationBuilder.AddForeignKey(
                name: "FK_Completions_Staff_StaffId",
                schema: "Training",
                table: "Completions",
                column: "StaffId",
                principalTable: "Staff",
                principalColumn: "StaffId");

            migrationBuilder.AddForeignKey(
                name: "FK_Faculties_Memberships_Staff_StaffId",
                table: "Faculties_Memberships",
                column: "StaffId",
                principalTable: "Staff",
                principalColumn: "StaffId");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupTutorials_Roll_Staff_StaffId",
                table: "GroupTutorials_Roll",
                column: "StaffId",
                principalTable: "Staff",
                principalColumn: "StaffId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GroupTutorials_Teachers_Staff_StaffId",
                table: "GroupTutorials_Teachers",
                column: "StaffId",
                principalTable: "Staff",
                principalColumn: "StaffId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MSTeamOperations_Staff_StaffId",
                table: "MSTeamOperations",
                column: "StaffId",
                principalTable: "Staff",
                principalColumn: "StaffId");

            migrationBuilder.AddForeignKey(
                name: "FK_MSTeamOperations_Staff_TeacherEmployedMSTeamOperation_StaffId",
                table: "MSTeamOperations",
                column: "TeacherEmployedMSTeamOperation_StaffId",
                principalTable: "Staff",
                principalColumn: "StaffId");

            migrationBuilder.AddForeignKey(
                name: "FK_MSTeamOperations_Staff_TeacherMSTeamOperation_StaffId",
                table: "MSTeamOperations",
                column: "TeacherMSTeamOperation_StaffId",
                principalTable: "Staff",
                principalColumn: "StaffId");

            migrationBuilder.AddForeignKey(
                name: "FK_Offerings_Teachers_Staff_StaffId",
                table: "Offerings_Teachers",
                column: "StaffId",
                principalTable: "Staff",
                principalColumn: "StaffId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkFlows_Actions_Staff_AssignedToId",
                table: "WorkFlows_Actions",
                column: "AssignedToId",
                principalTable: "Staff",
                principalColumn: "StaffId");
        }
    }
}
