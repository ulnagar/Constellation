using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    /// <inheritdoc />
    public partial class ImplementWorkFlows : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SectionId",
                table: "Offerings_Resources",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "WorkFlows_Cases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DetailId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkFlows_Cases", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkFlows_Actions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CaseId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ParentActionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AssignedToId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    AssignedTo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ActionType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Details = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Confirmed = table.Column<bool>(type: "bit", nullable: true),
                    OfferingId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OfferingName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NotRequired = table.Column<bool>(type: "bit", nullable: true),
                    IncidentNumber = table.Column<int>(type: "int", nullable: true),
                    DateOccurred = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ParentName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SenderName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SenderEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Subject = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    HasAttachments = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkFlows_Actions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkFlows_Actions_Offerings_Offerings_OfferingId",
                        column: x => x.OfferingId,
                        principalTable: "Offerings_Offerings",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WorkFlows_Actions_Staff_AssignedToId",
                        column: x => x.AssignedToId,
                        principalTable: "Staff",
                        principalColumn: "StaffId");
                    table.ForeignKey(
                        name: "FK_WorkFlows_Actions_WorkFlows_Cases_CaseId",
                        column: x => x.CaseId,
                        principalTable: "WorkFlows_Cases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkFlows_CaseDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CaseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CaseDetailType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StudentId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Grade = table.Column<int>(type: "int", nullable: true),
                    SchoolCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SchoolName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AttendanceValueId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PeriodLabel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Severity = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PerMinuteYearToDatePercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PerMinuteWeekPercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PerDayYearToDatePercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PerDayWeekPercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkFlows_CaseDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkFlows_CaseDetails_WorkFlows_Cases_CaseId",
                        column: x => x.CaseId,
                        principalTable: "WorkFlows_Cases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkFlows_ActionNotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ActionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SubmittedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkFlows_ActionNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkFlows_ActionNotes_WorkFlows_Actions_ActionId",
                        column: x => x.ActionId,
                        principalTable: "WorkFlows_Actions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkFlows_Actions_InterviewAttendees",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ActionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkFlows_Actions_InterviewAttendees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkFlows_Actions_InterviewAttendees_WorkFlows_Actions_ActionId",
                        column: x => x.ActionId,
                        principalTable: "WorkFlows_Actions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkFlows_Actions_Recipients",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OwnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkFlows_Actions_Recipients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkFlows_Actions_Recipients_WorkFlows_Actions_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "WorkFlows_Actions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkFlows_ActionNotes_ActionId",
                table: "WorkFlows_ActionNotes",
                column: "ActionId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkFlows_Actions_AssignedToId",
                table: "WorkFlows_Actions",
                column: "AssignedToId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkFlows_Actions_CaseId",
                table: "WorkFlows_Actions",
                column: "CaseId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkFlows_Actions_OfferingId",
                table: "WorkFlows_Actions",
                column: "OfferingId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkFlows_Actions_InterviewAttendees_ActionId",
                table: "WorkFlows_Actions_InterviewAttendees",
                column: "ActionId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkFlows_Actions_Recipients_OwnerId",
                table: "WorkFlows_Actions_Recipients",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkFlows_CaseDetails_CaseId",
                table: "WorkFlows_CaseDetails",
                column: "CaseId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkFlows_ActionNotes");

            migrationBuilder.DropTable(
                name: "WorkFlows_Actions_InterviewAttendees");

            migrationBuilder.DropTable(
                name: "WorkFlows_Actions_Recipients");

            migrationBuilder.DropTable(
                name: "WorkFlows_CaseDetails");

            migrationBuilder.DropTable(
                name: "WorkFlows_Actions");

            migrationBuilder.DropTable(
                name: "WorkFlows_Cases");

            migrationBuilder.DropColumn(
                name: "SectionId",
                table: "Offerings_Resources");
        }
    }
}
