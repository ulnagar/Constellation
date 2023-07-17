using System;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using OfficeOpenXml.Style;

#nullable disable

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    /// <inheritdoc />
    public partial class ConvertAbsencesToAggregate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Absences Table

            migrationBuilder.DropIndex(
                name: "IX_Absences_OfferingId",
                table: "Absences");

            migrationBuilder.DropIndex(
                name: "IX_Absences_StudentId",
                table: "Absences");

            migrationBuilder.DropForeignKey(
                name: "FK_Absences_Offerings_OfferingId",
                table: "Absences");

            migrationBuilder.DropForeignKey(
                name: "FK_Absences_Students_StudentId",
                table: "Absences");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Absences", 
                table: "Absences");

            migrationBuilder.RenameTable(
                name: "Absences",
                newName: "Absences_Absences");

            migrationBuilder.RenameColumn(
                name: "DateScanned",
                table: "Absences_Absences",
                newName: "FirstSeen");

            migrationBuilder.DropColumn(
                name: "ExternalExplanation",
                table: "Absences_Absences");

            migrationBuilder.DropColumn(
                name: "ExternalExplanationSource",
                table: "Absences_Absences");

            migrationBuilder.DropColumn(
                name: "ExternallyExplained",
                table: "Absences_Absences");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Absences_Absences",
                table: "Absences_Absences",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Absences_Absences_Offerings_OfferingId",
                table: "Absences_Absences",
                column: "OfferingId",
                principalTable: "Offerings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Absences_Absences_Students_StudentId",
                table: "Absences_Absences",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Absences_Absences_OfferingId",
                table: "Absences_Absences",
                column: "OfferingId");

            migrationBuilder.CreateIndex(
                name: "IX_Absences_Absences_StudentId",
                table: "Absences_Absences",
                column: "StudentId");

            // AbsenceNotifications table

            migrationBuilder.DropIndex(
                name: "IX_AbsenceNotification_AbsenceId",
                table: "AbsenceNotification");

            migrationBuilder.DropForeignKey(
                name: "FK_AbsenceNotification_Absences_AbsenceId",
                table: "AbsenceNotification");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AbsenceNotification",
                table: "AbsenceNotification");

            migrationBuilder.RenameTable(
                name: "AbsenceNotification",
                newName: "Absences_Notifications");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Absences_Notifications",
                table: "Absences_Notifications",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Absences_Notifications_Absences_Absences_AbsenceId",
                table: "Absences_Notifications",
                column: "AbsenceId",
                principalTable: "Absences_Absences",
                principalColumn: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Absences_Notifications_AbsenceId",
                table: "Absences_Notifications",
                column: "AbsenceId");

            // AbsenceResponse table

            migrationBuilder.DropIndex(
                name: "IX_AbsenceResponse_AbsenceId",
                table: "AbsenceResponse");

            migrationBuilder.DropForeignKey(
                name: "FK_AbsenceResponse_Absences_AbsenceId",
                table: "AbsenceResponse");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AbsenceResponse",
                table: "AbsenceResponse");

            migrationBuilder.RenameTable(
                name: "AbsenceResponse",
                newName: "Absences_Responses");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Absences_Responses",
                table: "Absences_Responses",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Absences_Responses_Absences_Absences_AbsenceId",
                table: "Absences_Responses",
                column: "AbsenceId",
                principalTable: "Absences_Absences",
                principalColumn: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Absences_Responses_AbsenceId",
                table: "Absences_Responses",
                column: "AbsenceId");

            // ClassworkNotifications table

            migrationBuilder.DropIndex(
                name: "IX_ClassworkNotifications_OfferingId",
                table: "ClassworkNotifications");

            migrationBuilder.DropIndex(
                name: "IX_ClassworkNotifications_StaffId",
                table: "ClassworkNotifications");

            migrationBuilder.DropForeignKey(
                name: "FK_ClassworkNotifications_Offerings_OfferingId",
                table: "ClassworkNotifications");

            migrationBuilder.DropForeignKey(
                name: "FK_ClassworkNotifications_Staff_StaffId",
                table: "ClassworkNotifications");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ClassworkNotifications",
                table: "ClassworkNotifications");

            migrationBuilder.RenameTable(
                name: "ClassworkNotifications",
                newName: "MissedWork_ClassworkNotifications");

            migrationBuilder.AddColumn<string>(
                name: "CompletedBy",
                table: "MissedWork_ClassworkNotifications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_MissedWork_ClassworkNotifications",
                table: "MissedWork_ClassworkNotifications",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MissedWork_ClassworkNotifications_Offerings_OfferingId",
                table: "MissedWork_ClassworkNotifications",
                column: "OfferingId",
                principalTable: "Offerings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MissedWork_ClassworkNotifications_Staff_StaffId",
                table: "MissedWork_ClassworkNotifications",
                column: "StaffId",
                principalTable: "Staff",
                principalColumn: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_MissedWork_ClassworkNotifications_OfferingId",
                table: "MissedWork_ClassworkNotifications",
                column: "OfferingId");

            migrationBuilder.CreateIndex(
                name: "IX_MissedWork_ClassworkNotifications_StaffId",
                table: "MissedWork_ClassworkNotifications",
                column: "StaffId");

            // AbsenceClassworkNotification table

            migrationBuilder.DropForeignKey(
                name: "FK_AbsenceClassworkNotification_ClassworkNotifications_ClassworkNotificationsId",
                table: "AbsenceClassworkNotification");

            migrationBuilder.DropForeignKey(
                name: "FK_AbsenceClassworkNotification_Absences_AbsencesId",
                table: "AbsenceClassworkNotification");

            migrationBuilder.RenameColumn(
                name: "ClassworkNotificationsId",
                table: "AbsenceClassworkNotification",
                newName: "ClassworkNotificationId");

            migrationBuilder.RenameIndex(
                name: "IX_AbsenceClassworkNotification_ClassworkNotificationsId",
                table: "AbsenceClassworkNotification",
                newName: "IX_AbsenceClassworkNotification_ClassworkNotificationId");

            migrationBuilder.AddForeignKey(
                name: "FK_AbsenceClassworkNotification_Absences_Absences_AbsencesId",
                table: "AbsenceClassworkNotification",
                column: "AbsencesId",
                principalTable: "Absences_Absences",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AbsenceClassworkNotification_MissedWork_ClassworkNotifications_ClassworkNotificationId",
                table: "AbsenceClassworkNotification",
                column: "ClassworkNotificationId",
                principalTable: "MissedWork_ClassworkNotifications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            // ClassworkNotificationStaff

            migrationBuilder.DropForeignKey(
                name: "FK_ClassworkNotificationStaff_ClassworkNotifications_ClassworkNotificationsId",
                table: "ClassworkNotificationStaff");

            migrationBuilder.RenameColumn(
                name: "ClassworkNotificationsId",
                table: "ClassworkNotificationStaff",
                newName: "ClassworkNotificationId");

            migrationBuilder.AddForeignKey(
                name: "FK_ClassworkNotificationStaff_MissedWork_ClassworkNotifications_ClassworkNotificationId",
                table: "ClassworkNotificationStaff",
                column: "ClassworkNotificationId",
                principalTable: "MissedWork_ClassworkNotifications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Absences Table

            migrationBuilder.DropIndex(
                name: "IX_Absences_Absences_StudentId",
                table: "Absences_Absences");

            migrationBuilder.DropIndex(
                name: "IX_Absences_Absences_OfferingId",
                table: "Absences_Absences");

            migrationBuilder.DropForeignKey(
                name: "FK_Absences_Absences_Students_StudentId",
                table: "Absences_Absences");

            migrationBuilder.DropForeignKey(
                name: "FK_Absences_Absences_Offerings_OfferingId",
                table: "Absences_Absences");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Absences_Absences",
                table: "Absences_Absences");

            migrationBuilder.AddColumn<string>(
                name: "ExternalExplanation",
                table: "Absences_Absences",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalExplanationSource",
                table: "Absences_Absences",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ExternallyExplained",
                table: "Absences_Absences",
                type: "bit",
                nullable: false);

            migrationBuilder.RenameColumn(
                name: "FirstSeen",
                table: "Absences_Absences",
                newName: "DateScanned");

            migrationBuilder.RenameTable(
                name: "Absences_Absences",
                newName: "Absences");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Absences",
                table: "Absences",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Absences_Students_StudentId",
                table: "Absences",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "StudentId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Absences_Offerings_OfferingId",
                table: "Absences",
                column: "OfferingId",
                principalTable: "Offerings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.CreateIndex(
                name: "IX_Absences_OfferingId",
                table: "Absences",
                column: "OfferingId");

            migrationBuilder.CreateIndex(
                name: "IX_Absences_StudentId",
                table: "Absences",
                column: "StudentId");

            // AbsenceNotifications table

            migrationBuilder.DropIndex(
                name: "IX_Absences_Notifications_AbsenceId",
                table: "Absences_Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Absences_Notifications_Absences_Absences_AbsenceId",
                table: "Absences_Notifications");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Absences_Notifications",
                table: "Absences_Notifications");

            migrationBuilder.RenameTable(
                name: "Absences_Notifications",
                newName: "AbsenceNotifications");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AbsenceNotification",
                table: "AbsenceNotification",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AbsenceNotification_Absences_AbsenceId",
                table: "AbsenceNotification",
                column: "AbsenceId",
                principalTable: "Absences",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.CreateIndex(
                name: "IX_AbsenceNotification_AbsenceId",
                table: "AbsenceNotification",
                column: "AbsenceId");

            // AbsenceResponse table

            migrationBuilder.DropIndex(
                name: "IX_Absences_Responses_AbsenceId",
                table: "Absences_Responses");

            migrationBuilder.DropForeignKey(
                name: "FK_Absences_Responses_Absences_Absences_AbsenceId",
                table: "Absences_Responses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Absences_Responses",
                table: "Absences_Responses");

            migrationBuilder.RenameTable(
                name: "Absences_Responses",
                newName: "AbsenceResponse");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AbsenceResponse",
                table: "AbsenceResponse",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AbsenceResponse_Absences_AbsenceId",
                table: "AbsenceResponse",
                column: "AbsenceId",
                principalTable: "Absences",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.CreateIndex(
                name: "IX_AbsenceResponse_AbsenceId",
                table: "AbsenceResponse",
                column: "AbsenceId");

            // ClassworkNotifications table

            migrationBuilder.DropIndex(
                name: "IX_MissedWork_ClassworkNotifications_OfferingId",
                table: "MissedWork_ClassworkNotifications");

            migrationBuilder.DropIndex(
                name: "IX_MissedWork_ClassworkNotifications_StaffId",
                table: "MissedWork_ClassworkNotifications");

            migrationBuilder.DropForeignKey(
                name: "FK_MissedWork_ClassworkNotifications_Offerings_OfferingId",
                table: "MissedWork_ClassworkNotifications");

            migrationBuilder.DropForeignKey(
                name: "FK_MissedWork_ClassworkNotifications_Staff_StaffId",
                table: "MissedWork_ClassworkNotifications");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MissedWork_ClassworkNotifications",
                table: "MissedWork_ClassworkNotifications");

            migrationBuilder.DropColumn(
                name: "CompletedBy",
                table: "MissedWork_ClassworkNotifications");

            migrationBuilder.RenameTable(
                name: "MissedWork_ClassworkNotifications",
                newName: "ClassworkNotifications");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ClassworkNotifications",
                table: "ClassworkNotifications",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ClassworkNotifications_Staff_StaffId",
                table: "ClassworkNotifications",
                column: "StaffId",
                principalTable: "Staff",
                principalColumn: "StaffId");

            migrationBuilder.AddForeignKey(
                name: "FK_ClassworkNotifications_Offerings_OfferingId",
                table: "ClassworkNotifications",
                column: "OfferingId",
                principalTable: "Offerings",
                principalColumn: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_ClassworkNotifications_OfferingId",
                table: "ClassworkNotifications",
                column: "OfferingId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassworkNotifications_StaffId",
                table: "ClassworkNotifications",
                column: "StaffId");

            // AbsenceClassworkNotification table

            migrationBuilder.DropForeignKey(
                name: "FK_AbsenceClassworkNotification_MissedWork_ClassworkNotifications_ClassworkNotificationId",
                table: "AbsenceClassworkNotification");

            migrationBuilder.DropForeignKey(
                name: "FK_AbsenceClassworkNotification_Absences_Absences_AbsencesId",
                table: "AbsenceClassworkNotification");

            migrationBuilder.RenameIndex(
                name: "IX_AbsenceClassworkNotification_ClassworkNotificationId",
                table: "AbsenceClassworkNotification",
                newName: "IX_AbsenceClassworkNotification_ClassworkNotificationsId");

            migrationBuilder.RenameColumn(
                name: "ClassworkNotificationId",
                table: "AbsenceClassworkNotification",
                newName: "ClassworkNotificationsId");

            migrationBuilder.AddForeignKey(
                name: "FK_AbsenceClassworkNotification_Absences_AbsencesId",
                table: "AbsenceClassworkNotification",
                column: "AbsencesId",
                principalTable: "Absences",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AbsenceClassworkNotification_ClassworkNotifications_ClassworkNotificationsId",
                table: "AbsenceClassworkNotification",
                column: "ClassworkNotificationsId",
                principalTable: "ClassworkNotifications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            // ClassworkNotificationStaff

            migrationBuilder.DropForeignKey(
                name: "FK_ClassworkNotificationStaff_MissedWork_ClassworkNotifications_ClassworkNotificationId",
                table: "ClassworkNotificationStaff");

            migrationBuilder.RenameColumn(
                name: "ClassworkNotificationId",
                table: "ClassworkNotificationStaff",
                newName: "ClassworkNotificationsId");

            migrationBuilder.AddForeignKey(
                name: "FK_ClassworkNotificationStaff_ClassworkNotifications_ClassworkNotificationsId",
                table: "ClassworkNotificationStaff",
                column: "ClassworkNotificationsId",
                principalTable: "ClassworkNotifications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
