using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    /// <inheritdoc />
    public partial class RemoveMissedWorkModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AbsenceClassworkNotification");

            migrationBuilder.DropTable(
                name: "ClassworkNotificationStaff");

            migrationBuilder.DropTable(
                name: "MissedWork_ClassworkNotifications");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MissedWork_ClassworkNotifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AbsenceDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GeneratedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsCovered = table.Column<bool>(type: "bit", nullable: false),
                    OfferingId = table.Column<int>(type: "int", nullable: false),
                    StaffId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MissedWork_ClassworkNotifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MissedWork_ClassworkNotifications_Offerings_OfferingId",
                        column: x => x.OfferingId,
                        principalTable: "Offerings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MissedWork_ClassworkNotifications_Staff_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Staff",
                        principalColumn: "StaffId");
                });

            migrationBuilder.CreateTable(
                name: "AbsenceClassworkNotification",
                columns: table => new
                {
                    AbsencesId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClassworkNotificationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbsenceClassworkNotification", x => new { x.AbsencesId, x.ClassworkNotificationId });
                    table.ForeignKey(
                        name: "FK_AbsenceClassworkNotification_Absences_Absences_AbsencesId",
                        column: x => x.AbsencesId,
                        principalTable: "Absences_Absences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AbsenceClassworkNotification_MissedWork_ClassworkNotifications_ClassworkNotificationId",
                        column: x => x.ClassworkNotificationId,
                        principalTable: "MissedWork_ClassworkNotifications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClassworkNotificationStaff",
                columns: table => new
                {
                    ClassworkNotificationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TeachersStaffId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassworkNotificationStaff", x => new { x.ClassworkNotificationId, x.TeachersStaffId });
                    table.ForeignKey(
                        name: "FK_ClassworkNotificationStaff_MissedWork_ClassworkNotifications_ClassworkNotificationId",
                        column: x => x.ClassworkNotificationId,
                        principalTable: "MissedWork_ClassworkNotifications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClassworkNotificationStaff_Staff_TeachersStaffId",
                        column: x => x.TeachersStaffId,
                        principalTable: "Staff",
                        principalColumn: "StaffId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AbsenceClassworkNotification_ClassworkNotificationId",
                table: "AbsenceClassworkNotification",
                column: "ClassworkNotificationId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassworkNotificationStaff_TeachersStaffId",
                table: "ClassworkNotificationStaff",
                column: "TeachersStaffId");

            migrationBuilder.CreateIndex(
                name: "IX_MissedWork_ClassworkNotifications_OfferingId",
                table: "MissedWork_ClassworkNotifications",
                column: "OfferingId");

            migrationBuilder.CreateIndex(
                name: "IX_MissedWork_ClassworkNotifications_StaffId",
                table: "MissedWork_ClassworkNotifications",
                column: "StaffId");
        }
    }
}
