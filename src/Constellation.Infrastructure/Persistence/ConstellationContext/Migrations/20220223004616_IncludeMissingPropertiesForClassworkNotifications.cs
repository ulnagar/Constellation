using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    public partial class IncludeMissingPropertiesForClassworkNotifications : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Absences_ClassworkNotifications_ClassworkNotificationId",
                table: "Absences");

            migrationBuilder.DropForeignKey(
                name: "FK_Covers_ClassworkNotifications_ClassworkNotificationId",
                table: "Covers");

            migrationBuilder.DropForeignKey(
                name: "FK_Staff_ClassworkNotifications_ClassworkNotificationId",
                table: "Staff");

            migrationBuilder.DropIndex(
                name: "IX_Staff_ClassworkNotificationId",
                table: "Staff");

            migrationBuilder.DropIndex(
                name: "IX_Covers_ClassworkNotificationId",
                table: "Covers");

            migrationBuilder.DropIndex(
                name: "IX_Absences_ClassworkNotificationId",
                table: "Absences");

            migrationBuilder.DropColumn(
                name: "ClassworkNotificationId",
                table: "Staff");

            migrationBuilder.DropColumn(
                name: "ClassworkNotificationId",
                table: "Covers");

            migrationBuilder.DropColumn(
                name: "ClassworkNotificationId",
                table: "Absences");

            migrationBuilder.CreateTable(
                name: "AbsenceClassworkNotification",
                columns: table => new
                {
                    AbsencesId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClassworkNotificationsId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbsenceClassworkNotification", x => new { x.AbsencesId, x.ClassworkNotificationsId });
                    table.ForeignKey(
                        name: "FK_AbsenceClassworkNotification_Absences_AbsencesId",
                        column: x => x.AbsencesId,
                        principalTable: "Absences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AbsenceClassworkNotification_ClassworkNotifications_ClassworkNotificationsId",
                        column: x => x.ClassworkNotificationsId,
                        principalTable: "ClassworkNotifications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClassCoverClassworkNotification",
                columns: table => new
                {
                    ClassworkNotificationsId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CoversId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassCoverClassworkNotification", x => new { x.ClassworkNotificationsId, x.CoversId });
                    table.ForeignKey(
                        name: "FK_ClassCoverClassworkNotification_ClassworkNotifications_ClassworkNotificationsId",
                        column: x => x.ClassworkNotificationsId,
                        principalTable: "ClassworkNotifications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClassCoverClassworkNotification_Covers_CoversId",
                        column: x => x.CoversId,
                        principalTable: "Covers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClassworkNotificationStaff",
                columns: table => new
                {
                    ClassworkNotificationsId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TeachersStaffId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassworkNotificationStaff", x => new { x.ClassworkNotificationsId, x.TeachersStaffId });
                    table.ForeignKey(
                        name: "FK_ClassworkNotificationStaff_ClassworkNotifications_ClassworkNotificationsId",
                        column: x => x.ClassworkNotificationsId,
                        principalTable: "ClassworkNotifications",
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
                name: "IX_AbsenceClassworkNotification_ClassworkNotificationsId",
                table: "AbsenceClassworkNotification",
                column: "ClassworkNotificationsId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassCoverClassworkNotification_CoversId",
                table: "ClassCoverClassworkNotification",
                column: "CoversId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassworkNotificationStaff_TeachersStaffId",
                table: "ClassworkNotificationStaff",
                column: "TeachersStaffId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AbsenceClassworkNotification");

            migrationBuilder.DropTable(
                name: "ClassCoverClassworkNotification");

            migrationBuilder.DropTable(
                name: "ClassworkNotificationStaff");

            migrationBuilder.AddColumn<Guid>(
                name: "ClassworkNotificationId",
                table: "Staff",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ClassworkNotificationId",
                table: "Covers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ClassworkNotificationId",
                table: "Absences",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Staff_ClassworkNotificationId",
                table: "Staff",
                column: "ClassworkNotificationId");

            migrationBuilder.CreateIndex(
                name: "IX_Covers_ClassworkNotificationId",
                table: "Covers",
                column: "ClassworkNotificationId");

            migrationBuilder.CreateIndex(
                name: "IX_Absences_ClassworkNotificationId",
                table: "Absences",
                column: "ClassworkNotificationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Absences_ClassworkNotifications_ClassworkNotificationId",
                table: "Absences",
                column: "ClassworkNotificationId",
                principalTable: "ClassworkNotifications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Covers_ClassworkNotifications_ClassworkNotificationId",
                table: "Covers",
                column: "ClassworkNotificationId",
                principalTable: "ClassworkNotifications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Staff_ClassworkNotifications_ClassworkNotificationId",
                table: "Staff",
                column: "ClassworkNotificationId",
                principalTable: "ClassworkNotifications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
