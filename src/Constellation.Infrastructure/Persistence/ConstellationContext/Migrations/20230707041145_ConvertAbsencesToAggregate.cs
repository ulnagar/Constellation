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
             
            migrationBuilder.DropForeignKey(
                name: "FK_AbsenceClassworkNotification_Absences_AbsencesId",
                table: "AbsenceClassworkNotification");

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
                name: "FK_ClassworkNotificationStaff_ClassworkNotifications_ClassworkNotificationsId",
                table: "ClassworkNotificationStaff");

            migrationBuilder.RenameColumn(
                name: "ClassworkNotificationsId",
                table: "ClassworkNotificationStaff",
                newName: "ClassworkNotificationId");

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
            migrationBuilder.DropForeignKey(
                name: "FK_AbsenceClassworkNotification_Absences_Absences_AbsencesId",
                table: "AbsenceClassworkNotification");

            migrationBuilder.DropForeignKey(
                name: "FK_AbsenceClassworkNotification_MissedWork_ClassworkNotifications_ClassworkNotificationId",
                table: "AbsenceClassworkNotification");

            migrationBuilder.DropForeignKey(
                name: "FK_ClassworkNotificationStaff_MissedWork_ClassworkNotifications_ClassworkNotificationId",
                table: "ClassworkNotificationStaff");

            migrationBuilder.DropTable(
                name: "Absences_Notifications");

            migrationBuilder.DropTable(
                name: "Absences_Responses");

            migrationBuilder.DropTable(
                name: "MissedWork_ClassworkNotifications");

            migrationBuilder.DropTable(
                name: "Absences_Absences");

            migrationBuilder.RenameColumn(
                name: "ClassworkNotificationId",
                table: "ClassworkNotificationStaff",
                newName: "ClassworkNotificationsId");

            migrationBuilder.RenameColumn(
                name: "ClassworkNotificationId",
                table: "AbsenceClassworkNotification",
                newName: "ClassworkNotificationsId");

            migrationBuilder.RenameIndex(
                name: "IX_AbsenceClassworkNotification_ClassworkNotificationId",
                table: "AbsenceClassworkNotification",
                newName: "IX_AbsenceClassworkNotification_ClassworkNotificationsId");

            migrationBuilder.CreateTable(
                name: "Absences",
                columns: table => new
                {
                    //Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    //OfferingId = table.Column<int>(type: "int", nullable: false),
                    //StudentId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    //AbsenceLength = table.Column<int>(type: "int", nullable: false),
                    //AbsenceReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    //AbsenceTimeframe = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    //Date = table.Column<DateTime>(type: "datetime2", nullable: false),

                    // Matches FirstSeen in new table
                    //DateScanned = table.Column<DateTime>(type: "datetime2", nullable: false),
                    //EndTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    ExternalExplanation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExternalExplanationSource = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExternallyExplained = table.Column<bool>(type: "bit", nullable: false),
                    //LastSeen = table.Column<DateTime>(type: "datetime2", nullable: false),
                    //PeriodName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    //PeriodTimeframe = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    //StartTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    //Type = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Absences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Absences_Offerings_OfferingId",
                        column: x => x.OfferingId,
                        principalTable: "Offerings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Absences_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "StudentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClassworkNotifications",
                columns: table => new
                {
                    //Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    //OfferingId = table.Column<int>(type: "int", nullable: false),
                    //StaffId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    //AbsenceDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    //CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    //Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    //GeneratedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    //IsCovered = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassworkNotifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClassworkNotifications_Offerings_OfferingId",
                        column: x => x.OfferingId,
                        principalTable: "Offerings",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ClassworkNotifications_Staff_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Staff",
                        principalColumn: "StaffId");
                });

            migrationBuilder.CreateTable(
                name: "AbsenceNotification",
                columns: table => new
                {
                    //Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    //AbsenceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    //ConfirmedDelivered = table.Column<bool>(type: "bit", nullable: false),
                    //DeliveredAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    //DeliveredMessageIds = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    //Message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    //OutgoingId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    //Recipients = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    //SentAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    //Type = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbsenceNotification", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AbsenceNotification_Absences_AbsenceId",
                        column: x => x.AbsenceId,
                        principalTable: "Absences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AbsenceResponse",
                columns: table => new
                {
                    //Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    //AbsenceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    //Explanation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    //Forwarded = table.Column<bool>(type: "bit", nullable: false),
                    //From = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    //ReceivedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    //Type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    //VerificationComment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    //VerificationStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    //VerifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    //Verifier = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbsenceResponse", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AbsenceResponse_Absences_AbsenceId",
                        column: x => x.AbsenceId,
                        principalTable: "Absences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AbsenceNotification_AbsenceId",
                table: "AbsenceNotification",
                column: "AbsenceId");

            migrationBuilder.CreateIndex(
                name: "IX_AbsenceResponse_AbsenceId",
                table: "AbsenceResponse",
                column: "AbsenceId");

            migrationBuilder.CreateIndex(
                name: "IX_Absences_OfferingId",
                table: "Absences",
                column: "OfferingId");

            migrationBuilder.CreateIndex(
                name: "IX_Absences_StudentId",
                table: "Absences",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassworkNotifications_OfferingId",
                table: "ClassworkNotifications",
                column: "OfferingId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassworkNotifications_StaffId",
                table: "ClassworkNotifications",
                column: "StaffId");

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
        }
    }
}
