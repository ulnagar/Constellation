using System;
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

            migrationBuilder.DropTable(
                name: "Staff");

            migrationBuilder.DropIndex(
                name: "IX_MSTeamOperations_TeacherEmployedMSTeamOperation_StaffId",
                table: "MSTeamOperations");

            migrationBuilder.DropIndex(
                name: "IX_Faculties_Memberships_StaffId",
                table: "Faculties_Memberships");

            migrationBuilder.DropColumn(
                name: "TeacherEmployedMSTeamOperation_StaffId",
                table: "MSTeamOperations");

            migrationBuilder.EnsureSchema(
                name: "Staff");

            migrationBuilder.AlterColumn<Guid>(
                name: "StaffId",
                table: "WorkFlows_CaseDetails",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "CreatedById",
                table: "WorkFlows_CaseDetails",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "AssignedToId",
                table: "WorkFlows_Actions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "StaffId",
                table: "Offerings_Teachers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "TeacherMSTeamOperation_StaffId",
                table: "MSTeamOperations",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "StaffId",
                table: "MSTeamOperations",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "StaffId1",
                table: "MSTeamOperations",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "StaffId",
                table: "GroupTutorials_Teachers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "StaffId",
                table: "GroupTutorials_Roll",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "StaffId",
                table: "Faculties_Memberships",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "StaffMemberId",
                table: "Faculties_Memberships",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "StaffId",
                schema: "Training",
                table: "Completions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "TeacherId",
                table: "Awards_StudentAwards",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "StaffId",
                schema: "Training",
                table: "Assignees",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<Guid>(
                name: "StaffId",
                table: "AspNetUsers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "Members",
                schema: "Staff",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<string>(type: "nvarchar(450)", nullable: true),
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
                    DateDeleted = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PreferredName = table.Column<string>(type: "nvarchar(max)", nullable: true)
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

            migrationBuilder.CreateIndex(
                name: "IX_MSTeamOperations_StaffId1",
                table: "MSTeamOperations",
                column: "StaffId1");

            migrationBuilder.CreateIndex(
                name: "IX_Faculties_Memberships_StaffMemberId",
                table: "Faculties_Memberships",
                column: "StaffMemberId");

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
