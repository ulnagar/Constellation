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
            // Migrate data

            // move all explanations in the absences table to separate records in the responses table
            migrationBuilder.Sql(
                @"INSERT INTO [dbo].AbsenceResponse
	                ([Id]
	                ,[AbsenceId]
	                ,[ReceivedAt]
	                ,[Type]
	                ,[From]
	                ,[Explanation]
	                ,[VerificationStatus]
	                ,[Verifier]
	                ,[VerifiedAt]
	                ,[VerificationComment]
	                ,[Forwarded])
                SELECT NEWID(), Id, DateScanned, 'System', ExternalExplanationSource, ExternalExplanation, 'NR', null, null, null, 1
                FROM [CONSTELLATION_DEV].[dbo].[Absences]
                where ExternalExplanation is not null;");

            // delete orphaned entries in the classworknotificationstaff table
            migrationBuilder.Sql(
                @"DELETE 
		            FROM CONSTELLATION_DEV.dbo.ClassworkNotificationStaff
		            WHERE ClassworkNotificationsId in (
			            SELECT ClassworkNotificationsId
			            FROM CONSTELLATION_DEV.dbo.ClassworkNotificationStaff cns
			            WHERE NOT EXISTS
			            (
				            SELECT 1 
				            FROM CONSTELLATION_DEV.dbo.ClassworkNotifications cn
				            WHERE cn.Id = cns.ClassworkNotificationsId
			            )
		            )");

            // Drop Indexes
            migrationBuilder.DropIndex(
                name: "IX_Absences_OfferingId",
                table: "Absences");

            migrationBuilder.DropIndex(
                name: "IX_Absences_StudentId",
                table: "Absences");

            migrationBuilder.DropIndex(
                name: "IX_AbsenceNotification_AbsenceId",
                table: "AbsenceNotification");

            migrationBuilder.DropIndex(
                name: "IX_AbsenceResponse_AbsenceId",
                table: "AbsenceResponse");

            migrationBuilder.DropIndex(
                name: "IX_ClassworkNotifications_OfferingId",
                table: "ClassworkNotifications");

            migrationBuilder.DropIndex(
                name: "IX_ClassworkNotifications_StaffId",
                table: "ClassworkNotifications");

            // Drop Foreign Keys

            migrationBuilder.DropForeignKey(
                name: "FK_Absences_Offerings_OfferingId",
                table: "Absences");

            migrationBuilder.DropForeignKey(
                name: "FK_Absences_Students_StudentId",
                table: "Absences");

            migrationBuilder.DropForeignKey(
                name: "FK_AbsenceNotification_Absences_AbsenceId",
                table: "AbsenceNotification");

            migrationBuilder.DropForeignKey(
                name: "FK_AbsenceResponse_Absences_AbsenceId",
                table: "AbsenceResponse");

            migrationBuilder.DropForeignKey(
                name: "FK_ClassworkNotifications_Offerings_OfferingId",
                table: "ClassworkNotifications");

            migrationBuilder.DropForeignKey(
                name: "FK_ClassworkNotifications_Staff_StaffId",
                table: "ClassworkNotifications");

            migrationBuilder.DropForeignKey(
                name: "FK_AbsenceClassworkNotification_ClassworkNotifications_ClassworkNotificationsId",
                table: "AbsenceClassworkNotification");

            migrationBuilder.DropForeignKey(
                name: "FK_AbsenceClassworkNotification_Absences_AbsencesId",
                table: "AbsenceClassworkNotification");

            migrationBuilder.DropForeignKey(
                name: "FK_ClassworkNotificationStaff_ClassworkNotifications_ClassworkNotificationsId",
                table: "ClassworkNotificationStaff");

            // Drop Primary keys

            migrationBuilder.DropPrimaryKey(
                name: "PK_Absences",
                table: "Absences");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AbsenceNotification",
                table: "AbsenceNotification");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AbsenceResponse",
                table: "AbsenceResponse");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ClassworkNotifications",
                table: "ClassworkNotifications");

            // Rename table

            migrationBuilder.RenameTable(
                name: "Absences",
                newName: "Absences_Absences");

            migrationBuilder.RenameTable(
                name: "AbsenceNotification",
                newName: "Absences_Notifications");

            migrationBuilder.RenameTable(
                name: "AbsenceResponse",
                newName: "Absences_Responses");

            migrationBuilder.RenameTable(
                name: "ClassworkNotifications",
                newName: "MissedWork_ClassworkNotifications");

            // Add/Update Columns

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

            migrationBuilder.AddColumn<bool>(
                name: "Explained",
                table: "Absences_Absences",
                type: "bit",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CompletedBy",
                table: "MissedWork_ClassworkNotifications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.RenameColumn(
                name: "ClassworkNotificationsId",
                table: "AbsenceClassworkNotification",
                newName: "ClassworkNotificationId");

            migrationBuilder.RenameColumn(
                name: "ClassworkNotificationsId",
                table: "ClassworkNotificationStaff",
                newName: "ClassworkNotificationId");

            // Add Primary Keys

            migrationBuilder.AddPrimaryKey(
                name: "PK_Absences_Absences",
                table: "Absences_Absences",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Absences_Notifications",
                table: "Absences_Notifications",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Absences_Responses",
                table: "Absences_Responses",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MissedWork_ClassworkNotifications",
                table: "MissedWork_ClassworkNotifications",
                column: "Id");

            // Add Foreign Keys

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

            migrationBuilder.AddForeignKey(
                name: "FK_Absences_Notifications_Absences_Absences_AbsenceId",
                table: "Absences_Notifications",
                column: "AbsenceId",
                principalTable: "Absences_Absences",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Absences_Responses_Absences_Absences_AbsenceId",
                table: "Absences_Responses",
                column: "AbsenceId",
                principalTable: "Absences_Absences",
                principalColumn: "Id");

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

            migrationBuilder.AddForeignKey(
                name: "FK_AbsenceClassworkNotification_Absences_Absences_AbsencesId",
                table: "AbsenceClassworkNotification",
                column: "AbsencesId",
                principalTable: "Absences_Absences",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_AbsenceClassworkNotification_MissedWork_ClassworkNotifications_ClassworkNotificationId",
                table: "AbsenceClassworkNotification",
                column: "ClassworkNotificationId",
                principalTable: "MissedWork_ClassworkNotifications",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_ClassworkNotificationStaff_MissedWork_ClassworkNotifications_ClassworkNotificationId",
                table: "ClassworkNotificationStaff",
                column: "ClassworkNotificationId",
                principalTable: "MissedWork_ClassworkNotifications",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            // Add Indexes

            migrationBuilder.CreateIndex(
                name: "IX_Absences_Absences_OfferingId",
                table: "Absences_Absences",
                column: "OfferingId");

            migrationBuilder.CreateIndex(
                name: "IX_Absences_Absences_StudentId",
                table: "Absences_Absences",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Absences_Notifications_AbsenceId",
                table: "Absences_Notifications",
                column: "AbsenceId");

            migrationBuilder.CreateIndex(
                name: "IX_Absences_Responses_AbsenceId",
                table: "Absences_Responses",
                column: "AbsenceId");

            migrationBuilder.CreateIndex(
                name: "IX_MissedWork_ClassworkNotifications_OfferingId",
                table: "MissedWork_ClassworkNotifications",
                column: "OfferingId");

            migrationBuilder.CreateIndex(
                name: "IX_MissedWork_ClassworkNotifications_StaffId",
                table: "MissedWork_ClassworkNotifications",
                column: "StaffId");

            migrationBuilder.RenameIndex(
                name: "IX_AbsenceClassworkNotification_ClassworkNotificationsId",
                table: "AbsenceClassworkNotification",
                newName: "IX_AbsenceClassworkNotification_ClassworkNotificationId");

            // Data migration

            // populate new explained column in absences table
            migrationBuilder.Sql(
                @"UPDATE [Absences_Absences]
                    SET Explained = 
	                    CASE
		                    WHEN [Absences_Absences].[Type] = 'Whole' AND (SELECT COUNT([Absences_Responses].[Id]) FROM [Absences_Responses] WHERE [Absences_Absences].[Id] = [Absences_Responses].[AbsenceId] AND [Absences_Responses].[Type] <> 'Student') > 0 THEN 1
		                    WHEN [Absences_Absences].[Type] = 'Partial' AND (SELECT COUNT([Absences_Responses].[Id]) FROM [Absences_Responses] WHERE [Absences_Absences].[Id] = [Absences_Responses].[AbsenceId] AND [Absences_Responses].[VerificationStatus] = 'Verified') > 0 THEN 1
		                    ELSE 0
	                    END");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop Indexes

            migrationBuilder.DropIndex(
                name: "IX_Absences_Absences_StudentId",
                table: "Absences_Absences");

            migrationBuilder.DropIndex(
                name: "IX_Absences_Absences_OfferingId",
                table: "Absences_Absences");

            migrationBuilder.DropIndex(
                name: "IX_Absences_Notifications_AbsenceId",
                table: "Absences_Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Absences_Responses_AbsenceId",
                table: "Absences_Responses");

            migrationBuilder.DropIndex(
                name: "IX_MissedWork_ClassworkNotifications_OfferingId",
                table: "MissedWork_ClassworkNotifications");

            migrationBuilder.DropIndex(
                name: "IX_MissedWork_ClassworkNotifications_StaffId",
                table: "MissedWork_ClassworkNotifications");

            // Drop Foreign Keys

            migrationBuilder.DropForeignKey(
                name: "FK_Absences_Absences_Students_StudentId",
                table: "Absences_Absences");

            migrationBuilder.DropForeignKey(
                name: "FK_Absences_Absences_Offerings_OfferingId",
                table: "Absences_Absences");

            migrationBuilder.DropForeignKey(
                name: "FK_Absences_Notifications_Absences_Absences_AbsenceId",
                table: "Absences_Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Absences_Responses_Absences_Absences_AbsenceId",
                table: "Absences_Responses");

            migrationBuilder.DropForeignKey(
                name: "FK_MissedWork_ClassworkNotifications_Offerings_OfferingId",
                table: "MissedWork_ClassworkNotifications");

            migrationBuilder.DropForeignKey(
                name: "FK_MissedWork_ClassworkNotifications_Staff_StaffId",
                table: "MissedWork_ClassworkNotifications");

            migrationBuilder.DropForeignKey(
                name: "FK_AbsenceClassworkNotification_MissedWork_ClassworkNotifications_ClassworkNotificationId",
                table: "AbsenceClassworkNotification");

            migrationBuilder.DropForeignKey(
                name: "FK_AbsenceClassworkNotification_Absences_Absences_AbsencesId",
                table: "AbsenceClassworkNotification");

            migrationBuilder.DropForeignKey(
                name: "FK_ClassworkNotificationStaff_MissedWork_ClassworkNotifications_ClassworkNotificationId",
                table: "ClassworkNotificationStaff");

            // Drop Primary Keys

            migrationBuilder.DropPrimaryKey(
                name: "PK_Absences_Absences",
                table: "Absences_Absences");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Absences_Notifications",
                table: "Absences_Notifications");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Absences_Responses",
                table: "Absences_Responses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MissedWork_ClassworkNotifications",
                table: "MissedWork_ClassworkNotifications");

            // Rename table

            migrationBuilder.RenameTable(
                name: "Absences_Absences",
                newName: "Absences");

            migrationBuilder.RenameTable(
                name: "Absences_Notifications",
                newName: "AbsenceNotifications");

            migrationBuilder.RenameTable(
                name: "Absences_Responses",
                newName: "AbsenceResponse");

            migrationBuilder.RenameTable(
                name: "MissedWork_ClassworkNotifications",
                newName: "ClassworkNotifications");

            // Add/Update Columns

            migrationBuilder.DropColumn(
                name: "Explained",
                table: "Absences");

            migrationBuilder.AddColumn<string>(
                name: "ExternalExplanation",
                table: "Absences",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalExplanationSource",
                table: "Absences",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ExternallyExplained",
                table: "Absences",
                type: "bit",
                nullable: false);

            migrationBuilder.RenameColumn(
                name: "FirstSeen",
                table: "Absences",
                newName: "DateScanned");

            migrationBuilder.DropColumn(
                name: "CompletedBy",
                table: "ClassworkNotifications");

            migrationBuilder.RenameColumn(
                name: "ClassworkNotificationId",
                table: "AbsenceClassworkNotification",
                newName: "ClassworkNotificationsId");

            migrationBuilder.RenameColumn(
                name: "ClassworkNotificationId",
                table: "ClassworkNotificationStaff",
                newName: "ClassworkNotificationsId");

            // Add Primary Keys

            migrationBuilder.AddPrimaryKey(
                name: "PK_Absences",
                table: "Absences",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AbsenceNotification",
                table: "AbsenceNotification",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AbsenceResponse",
                table: "AbsenceResponse",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ClassworkNotifications",
                table: "ClassworkNotifications",
                column: "Id");

            // Add Foreign Keys

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

            migrationBuilder.AddForeignKey(
                name: "FK_AbsenceNotification_Absences_AbsenceId",
                table: "AbsenceNotification",
                column: "AbsenceId",
                principalTable: "Absences",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AbsenceResponse_Absences_AbsenceId",
                table: "AbsenceResponse",
                column: "AbsenceId",
                principalTable: "Absences",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

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

            migrationBuilder.AddForeignKey(
                name: "FK_ClassworkNotificationStaff_ClassworkNotifications_ClassworkNotificationsId",
                table: "ClassworkNotificationStaff",
                column: "ClassworkNotificationsId",
                principalTable: "ClassworkNotifications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            // Add Indexes

            migrationBuilder.CreateIndex(
                name: "IX_Absences_OfferingId",
                table: "Absences",
                column: "OfferingId");

            migrationBuilder.CreateIndex(
                name: "IX_Absences_StudentId",
                table: "Absences",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_AbsenceNotification_AbsenceId",
                table: "AbsenceNotification",
                column: "AbsenceId");

            migrationBuilder.CreateIndex(
                name: "IX_AbsenceResponse_AbsenceId",
                table: "AbsenceResponse",
                column: "AbsenceId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassworkNotifications_OfferingId",
                table: "ClassworkNotifications",
                column: "OfferingId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassworkNotifications_StaffId",
                table: "ClassworkNotifications",
                column: "StaffId");

            migrationBuilder.RenameIndex(
                name: "IX_AbsenceClassworkNotification_ClassworkNotificationId",
                table: "AbsenceClassworkNotification",
                newName: "IX_AbsenceClassworkNotification_ClassworkNotificationsId");
        }
    }
}
