using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    /// <inheritdoc />
    public partial class ConvertSomeEntitiesToNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PartialAbsenceNotifications");

            migrationBuilder.DropTable(
                name: "PartialAbsenceResponses");

            migrationBuilder.DropTable(
                name: "PartialAbsenceVerifications");

            migrationBuilder.DropTable(
                name: "WholeAbsenceNotifications");

            migrationBuilder.DropTable(
                name: "WholeAbsenceResponses");

            migrationBuilder.DropTable(
                name: "PartialAbsences");

            migrationBuilder.DropTable(
                name: "WholeAbsences");

            migrationBuilder.AlterColumn<string>(
                name: "ModifiedBy",
                table: "WorkFlows_Cases",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DeletedBy",
                table: "WorkFlows_Cases",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "WorkFlows_Cases",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ModifiedBy",
                table: "WorkFlows_Cases",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "DeletedBy",
                table: "WorkFlows_Cases",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "WorkFlows_Cases",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateTable(
                name: "PartialAbsences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OfferingId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StudentId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateScanned = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExternallyExplained = table.Column<bool>(type: "bit", nullable: false),
                    LastSeen = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PartialAbsenceLength = table.Column<int>(type: "int", nullable: false),
                    PartialAbsenceTimeframe = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PeriodName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PeriodTimeframe = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartialAbsences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PartialAbsences_Offerings_Offerings_OfferingId",
                        column: x => x.OfferingId,
                        principalTable: "Offerings_Offerings",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PartialAbsences_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "StudentId");
                });

            migrationBuilder.CreateTable(
                name: "WholeAbsences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OfferingId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StudentId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateScanned = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExternallyExplained = table.Column<bool>(type: "bit", nullable: false),
                    LastSeen = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PeriodName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PeriodTimeframe = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WholeAbsences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WholeAbsences_Offerings_Offerings_OfferingId",
                        column: x => x.OfferingId,
                        principalTable: "Offerings_Offerings",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WholeAbsences_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "StudentId");
                });

            migrationBuilder.CreateTable(
                name: "PartialAbsenceNotifications",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AbsenceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OutgoingId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Recipients = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartialAbsenceNotifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PartialAbsenceNotifications_PartialAbsences_AbsenceId",
                        column: x => x.AbsenceId,
                        principalTable: "PartialAbsences",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PartialAbsenceResponses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AbsenceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VerifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Explanation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReceivedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Verification = table.Column<int>(type: "int", nullable: false),
                    VerifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VerifiedComment = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartialAbsenceResponses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PartialAbsenceResponses_PartialAbsences_AbsenceId",
                        column: x => x.AbsenceId,
                        principalTable: "PartialAbsences",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PartialAbsenceResponses_SchoolContacts_Contacts_VerifierId",
                        column: x => x.VerifierId,
                        principalTable: "SchoolContacts_Contacts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PartialAbsenceVerifications",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AbsenceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OutgoingId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Recipients = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartialAbsenceVerifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PartialAbsenceVerifications_PartialAbsences_AbsenceId",
                        column: x => x.AbsenceId,
                        principalTable: "PartialAbsences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WholeAbsenceNotifications",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AbsenceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConfirmedDelivered = table.Column<bool>(type: "bit", nullable: false),
                    DeliveredAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeliveredMessageIds = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OutgoingId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Recipients = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WholeAbsenceNotifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WholeAbsenceNotifications_WholeAbsences_AbsenceId",
                        column: x => x.AbsenceId,
                        principalTable: "WholeAbsences",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "WholeAbsenceResponses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AbsenceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Explanation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Forwarded = table.Column<bool>(type: "bit", nullable: false),
                    ForwardedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReceivedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReceivedFrom = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReceivedFromName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WholeAbsenceResponses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WholeAbsenceResponses_WholeAbsences_AbsenceId",
                        column: x => x.AbsenceId,
                        principalTable: "WholeAbsences",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_PartialAbsenceNotifications_AbsenceId",
                table: "PartialAbsenceNotifications",
                column: "AbsenceId");

            migrationBuilder.CreateIndex(
                name: "IX_PartialAbsenceResponses_AbsenceId",
                table: "PartialAbsenceResponses",
                column: "AbsenceId");

            migrationBuilder.CreateIndex(
                name: "IX_PartialAbsenceResponses_VerifierId",
                table: "PartialAbsenceResponses",
                column: "VerifierId");

            migrationBuilder.CreateIndex(
                name: "IX_PartialAbsences_OfferingId",
                table: "PartialAbsences",
                column: "OfferingId");

            migrationBuilder.CreateIndex(
                name: "IX_PartialAbsences_StudentId",
                table: "PartialAbsences",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_PartialAbsenceVerifications_AbsenceId",
                table: "PartialAbsenceVerifications",
                column: "AbsenceId");

            migrationBuilder.CreateIndex(
                name: "IX_WholeAbsenceNotifications_AbsenceId",
                table: "WholeAbsenceNotifications",
                column: "AbsenceId");

            migrationBuilder.CreateIndex(
                name: "IX_WholeAbsenceResponses_AbsenceId",
                table: "WholeAbsenceResponses",
                column: "AbsenceId");

            migrationBuilder.CreateIndex(
                name: "IX_WholeAbsences_OfferingId",
                table: "WholeAbsences",
                column: "OfferingId");

            migrationBuilder.CreateIndex(
                name: "IX_WholeAbsences_StudentId",
                table: "WholeAbsences",
                column: "StudentId");
        }
    }
}
