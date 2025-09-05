using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    /// <inheritdoc />
    public partial class AddAwardNominationNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Awards_NominationPeriods_Grades_Awards_NominationPeriods_PeriodId",
                table: "Awards_NominationPeriods_Grades");

            migrationBuilder.DropForeignKey(
                name: "FK_Awards_Nominations_Awards_NominationPeriods_PeriodId",
                table: "Awards_Nominations");

            migrationBuilder.DropForeignKey(
                name: "FK_Awards_Nominations_Students_StudentId",
                table: "Awards_Nominations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Awards_Nominations",
                table: "Awards_Nominations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Awards_NominationPeriods_Grades",
                table: "Awards_NominationPeriods_Grades");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Awards_NominationPeriods",
                table: "Awards_NominationPeriods");

            migrationBuilder.EnsureSchema(
                name: "AwardNominations");

            migrationBuilder.RenameTable(
                name: "Awards_Nominations",
                newName: "Nominations",
                newSchema: "AwardNominations");

            migrationBuilder.RenameTable(
                name: "Awards_NominationPeriods_Grades",
                newName: "PeriodGrades",
                newSchema: "AwardNominations");

            migrationBuilder.RenameTable(
                name: "Awards_NominationPeriods",
                newName: "Periods",
                newSchema: "AwardNominations");

            migrationBuilder.RenameIndex(
                name: "IX_Awards_Nominations_StudentId",
                schema: "AwardNominations",
                table: "Nominations",
                newName: "IX_Nominations_StudentId");

            migrationBuilder.RenameIndex(
                name: "IX_Awards_Nominations_PeriodId",
                schema: "AwardNominations",
                table: "Nominations",
                newName: "IX_Nominations_PeriodId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Nominations",
                schema: "AwardNominations",
                table: "Nominations",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PeriodGrades",
                schema: "AwardNominations",
                table: "PeriodGrades",
                columns: new[] { "PeriodId", "Grade" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Periods",
                schema: "AwardNominations",
                table: "Periods",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Notifications",
                schema: "AwardNominations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PeriodId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Nominations = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FromAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ToAddresses = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CcAddresses = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Subject = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_Periods_PeriodId",
                        column: x => x.PeriodId,
                        principalSchema: "AwardNominations",
                        principalTable: "Periods",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_PeriodId",
                schema: "AwardNominations",
                table: "Notifications",
                column: "PeriodId");

            migrationBuilder.AddForeignKey(
                name: "FK_Nominations_Periods_PeriodId",
                schema: "AwardNominations",
                table: "Nominations",
                column: "PeriodId",
                principalSchema: "AwardNominations",
                principalTable: "Periods",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Nominations_Students_StudentId",
                schema: "AwardNominations",
                table: "Nominations",
                column: "StudentId",
                principalSchema: "Students",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PeriodGrades_Periods_PeriodId",
                schema: "AwardNominations",
                table: "PeriodGrades",
                column: "PeriodId",
                principalSchema: "AwardNominations",
                principalTable: "Periods",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Nominations_Periods_PeriodId",
                schema: "AwardNominations",
                table: "Nominations");

            migrationBuilder.DropForeignKey(
                name: "FK_Nominations_Students_StudentId",
                schema: "AwardNominations",
                table: "Nominations");

            migrationBuilder.DropForeignKey(
                name: "FK_PeriodGrades_Periods_PeriodId",
                schema: "AwardNominations",
                table: "PeriodGrades");

            migrationBuilder.DropTable(
                name: "Notifications",
                schema: "AwardNominations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Periods",
                schema: "AwardNominations",
                table: "Periods");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PeriodGrades",
                schema: "AwardNominations",
                table: "PeriodGrades");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Nominations",
                schema: "AwardNominations",
                table: "Nominations");

            migrationBuilder.RenameTable(
                name: "Periods",
                schema: "AwardNominations",
                newName: "Awards_NominationPeriods");

            migrationBuilder.RenameTable(
                name: "PeriodGrades",
                schema: "AwardNominations",
                newName: "Awards_NominationPeriods_Grades");

            migrationBuilder.RenameTable(
                name: "Nominations",
                schema: "AwardNominations",
                newName: "Awards_Nominations");

            migrationBuilder.RenameIndex(
                name: "IX_Nominations_StudentId",
                table: "Awards_Nominations",
                newName: "IX_Awards_Nominations_StudentId");

            migrationBuilder.RenameIndex(
                name: "IX_Nominations_PeriodId",
                table: "Awards_Nominations",
                newName: "IX_Awards_Nominations_PeriodId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Awards_NominationPeriods",
                table: "Awards_NominationPeriods",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Awards_NominationPeriods_Grades",
                table: "Awards_NominationPeriods_Grades",
                columns: new[] { "PeriodId", "Grade" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Awards_Nominations",
                table: "Awards_Nominations",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Awards_NominationPeriods_Grades_Awards_NominationPeriods_PeriodId",
                table: "Awards_NominationPeriods_Grades",
                column: "PeriodId",
                principalTable: "Awards_NominationPeriods",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Awards_Nominations_Awards_NominationPeriods_PeriodId",
                table: "Awards_Nominations",
                column: "PeriodId",
                principalTable: "Awards_NominationPeriods",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Awards_Nominations_Students_StudentId",
                table: "Awards_Nominations",
                column: "StudentId",
                principalSchema: "Students",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
