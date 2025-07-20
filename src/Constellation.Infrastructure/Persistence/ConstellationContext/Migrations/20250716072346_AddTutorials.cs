using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    /// <inheritdoc />
    public partial class AddTutorials : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MSTeamOperations_GroupTutorials_Tutorial_TutorialId",
                table: "MSTeamOperations");

            migrationBuilder.DropIndex(
                name: "IX_MSTeamOperations_TutorialId",
                table: "MSTeamOperations");

            migrationBuilder.EnsureSchema(
                name: "Tutorials");

            migrationBuilder.AddColumn<Guid>(
                name: "GroupTutorialId",
                table: "MSTeamOperations",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "OfferingId",
                table: "Enrolments",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<string>(
                name: "EnrolmentType",
                table: "Enrolments",
                type: "nvarchar(21)",
                maxLength: 21,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(@"
                UPDATE [dto].[Enrolments]
                SET EnrolmentType = 'OfferingEnrolment'
                WHERE 1 = 1;");

            migrationBuilder.AddColumn<Guid>(
                name: "TutorialId",
                table: "Enrolments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Tutorials",
                schema: "Tutorials",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
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
                    table.PrimaryKey("PK_Tutorials", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Sessions",
                schema: "Tutorials",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TutorialId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Week = table.Column<int>(type: "int", nullable: true),
                    Day = table.Column<int>(type: "int", nullable: true),
                    StartTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    StaffId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sessions_Members_StaffId",
                        column: x => x.StaffId,
                        principalSchema: "Staff",
                        principalTable: "Members",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Sessions_Tutorials_TutorialId",
                        column: x => x.TutorialId,
                        principalSchema: "Tutorials",
                        principalTable: "Tutorials",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Teams",
                schema: "Tutorials",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TutorialId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TeamId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Teams_Tutorials_TutorialId",
                        column: x => x.TutorialId,
                        principalSchema: "Tutorials",
                        principalTable: "Tutorials",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_MSTeamOperations_GroupTutorialId",
                table: "MSTeamOperations",
                column: "GroupTutorialId");

            migrationBuilder.CreateIndex(
                name: "IX_MSTeamOperations_StudentId2",
                table: "MSTeamOperations",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Enrolments_TutorialId",
                table: "Enrolments",
                column: "TutorialId");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_StaffId",
                schema: "Tutorials",
                table: "Sessions",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_TutorialId",
                schema: "Tutorials",
                table: "Sessions",
                column: "TutorialId");

            migrationBuilder.CreateIndex(
                name: "IX_Teams_TutorialId",
                schema: "Tutorials",
                table: "Teams",
                column: "TutorialId");

            migrationBuilder.AddForeignKey(
                name: "FK_Enrolments_Tutorials_TutorialId",
                table: "Enrolments",
                column: "TutorialId",
                principalSchema: "Tutorials",
                principalTable: "Tutorials",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MSTeamOperations_GroupTutorials_Tutorial_GroupTutorialId",
                table: "MSTeamOperations",
                column: "GroupTutorialId",
                principalTable: "GroupTutorials_Tutorial",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MSTeamOperations_Students_StudentId2",
                table: "MSTeamOperations",
                column: "StudentId",
                principalSchema: "Students",
                principalTable: "Students",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Enrolments_Tutorials_TutorialId",
                table: "Enrolments");

            migrationBuilder.DropForeignKey(
                name: "FK_MSTeamOperations_GroupTutorials_Tutorial_GroupTutorialId",
                table: "MSTeamOperations");

            migrationBuilder.DropForeignKey(
                name: "FK_MSTeamOperations_Students_StudentId2",
                table: "MSTeamOperations");

            migrationBuilder.DropTable(
                name: "Sessions",
                schema: "Tutorials");

            migrationBuilder.DropTable(
                name: "Teams",
                schema: "Tutorials");

            migrationBuilder.DropTable(
                name: "Tutorials",
                schema: "Tutorials");

            migrationBuilder.DropIndex(
                name: "IX_MSTeamOperations_GroupTutorialId",
                table: "MSTeamOperations");

            migrationBuilder.DropIndex(
                name: "IX_MSTeamOperations_StudentId2",
                table: "MSTeamOperations");

            migrationBuilder.DropIndex(
                name: "IX_Enrolments_TutorialId",
                table: "Enrolments");

            migrationBuilder.DropColumn(
                name: "GroupTutorialId",
                table: "MSTeamOperations");

            migrationBuilder.DropColumn(
                name: "EnrolmentType",
                table: "Enrolments");

            migrationBuilder.DropColumn(
                name: "TutorialId",
                table: "Enrolments");

            migrationBuilder.AlterColumn<Guid>(
                name: "OfferingId",
                table: "Enrolments",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MSTeamOperations_TutorialId",
                table: "MSTeamOperations",
                column: "TutorialId");

            migrationBuilder.AddForeignKey(
                name: "FK_MSTeamOperations_GroupTutorials_Tutorial_TutorialId",
                table: "MSTeamOperations",
                column: "TutorialId",
                principalTable: "GroupTutorials_Tutorial",
                principalColumn: "Id");
        }
    }
}
