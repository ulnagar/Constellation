using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    /// <inheritdoc />
    public partial class RemoveOldTeamOperations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MSTeamOperations");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MSTeamOperations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false, defaultValueSql: "NEXT VALUE FOR [TeamsOperationId]"),
                    Action = table.Column<int>(type: "int", nullable: false),
                    CoverId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DateScheduled = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    PermissionLevel = table.Column<int>(type: "int", nullable: false),
                    UserType = table.Column<string>(type: "nvarchar(55)", maxLength: 55, nullable: false),
                    ContactId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TeamName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GroupTutorialId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TeamDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OfferingId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CasualId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FacultyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StudentMSTeamOperation_StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TeacherMSTeamOperation_StaffId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StudentEnrolledMSTeamOperation_StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TutorialId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StaffId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StaffId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TutorialCreatedMSTeamOperation_TeamDescription = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MSTeamOperations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MSTeamOperations_Faculties_Faculty_FacultyId",
                        column: x => x.FacultyId,
                        principalTable: "Faculties_Faculty",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MSTeamOperations_GroupTutorials_Tutorial_GroupTutorialId",
                        column: x => x.GroupTutorialId,
                        principalTable: "GroupTutorials_Tutorial",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MSTeamOperations_Members_StaffId",
                        column: x => x.StaffId,
                        principalSchema: "Staff",
                        principalTable: "Members",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MSTeamOperations_Members_StaffId1",
                        column: x => x.StaffId1,
                        principalSchema: "Staff",
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MSTeamOperations_Members_TeacherMSTeamOperation_StaffId",
                        column: x => x.TeacherMSTeamOperation_StaffId,
                        principalSchema: "Staff",
                        principalTable: "Members",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MSTeamOperations_Offerings_Offerings_OfferingId",
                        column: x => x.OfferingId,
                        principalTable: "Offerings_Offerings",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MSTeamOperations_SchoolContacts_Contacts_ContactId",
                        column: x => x.ContactId,
                        principalTable: "SchoolContacts_Contacts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MSTeamOperations_Students_StudentEnrolledMSTeamOperation_StudentId",
                        column: x => x.StudentEnrolledMSTeamOperation_StudentId,
                        principalSchema: "Students",
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MSTeamOperations_Students_StudentId1",
                        column: x => x.StudentId,
                        principalSchema: "Students",
                        principalTable: "Students",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MSTeamOperations_Students_StudentId2",
                        column: x => x.StudentId,
                        principalSchema: "Students",
                        principalTable: "Students",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MSTeamOperations_Students_StudentMSTeamOperation_StudentId",
                        column: x => x.StudentMSTeamOperation_StudentId,
                        principalSchema: "Students",
                        principalTable: "Students",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_MSTeamOperations_ContactId",
                table: "MSTeamOperations",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_MSTeamOperations_FacultyId",
                table: "MSTeamOperations",
                column: "FacultyId");

            migrationBuilder.CreateIndex(
                name: "IX_MSTeamOperations_GroupTutorialId",
                table: "MSTeamOperations",
                column: "GroupTutorialId");

            migrationBuilder.CreateIndex(
                name: "IX_MSTeamOperations_OfferingId",
                table: "MSTeamOperations",
                column: "OfferingId");

            migrationBuilder.CreateIndex(
                name: "IX_MSTeamOperations_StaffId",
                table: "MSTeamOperations",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_MSTeamOperations_StaffId1",
                table: "MSTeamOperations",
                column: "StaffId1");

            migrationBuilder.CreateIndex(
                name: "IX_MSTeamOperations_StudentEnrolledMSTeamOperation_StudentId",
                table: "MSTeamOperations",
                column: "StudentEnrolledMSTeamOperation_StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_MSTeamOperations_StudentId1",
                table: "MSTeamOperations",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_MSTeamOperations_StudentId2",
                table: "MSTeamOperations",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_MSTeamOperations_StudentMSTeamOperation_StudentId",
                table: "MSTeamOperations",
                column: "StudentMSTeamOperation_StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_MSTeamOperations_TeacherMSTeamOperation_StaffId",
                table: "MSTeamOperations",
                column: "TeacherMSTeamOperation_StaffId");
        }
    }
}
