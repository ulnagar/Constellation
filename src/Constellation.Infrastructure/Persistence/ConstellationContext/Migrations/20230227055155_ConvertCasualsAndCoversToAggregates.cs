using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    public partial class ConvertCasualsAndCoversToAggregates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AdobeConnectOperations_Casuals_CasualId",
                table: "AdobeConnectOperations");

            migrationBuilder.DropForeignKey(
                name: "FK_AdobeConnectOperations_Covers_CoverId",
                table: "AdobeConnectOperations");

            migrationBuilder.DropForeignKey(
                name: "FK_MSTeamOperations_Casuals_CasualId",
                table: "MSTeamOperations");

            migrationBuilder.DropForeignKey(
                name: "FK_MSTeamOperations_Covers_CoverId",
                table: "MSTeamOperations");

            migrationBuilder.DropForeignKey(
                name: "FK_MSTeamOperations_Covers_TeacherMSTeamOperation_CoverId",
                table: "MSTeamOperations");

            migrationBuilder.DropTable(
                name: "ClassCoverClassworkNotification");

            migrationBuilder.DropTable(
                name: "Covers");

            migrationBuilder.DropTable(
                name: "Casuals");

            migrationBuilder.DropIndex(
                name: "IX_MSTeamOperations_CasualId",
                table: "MSTeamOperations");

            migrationBuilder.DropIndex(
                name: "IX_MSTeamOperations_CoverId",
                table: "MSTeamOperations");

            migrationBuilder.DropIndex(
                name: "IX_MSTeamOperations_TeacherMSTeamOperation_CoverId",
                table: "MSTeamOperations");

            migrationBuilder.DropIndex(
                name: "IX_AdobeConnectOperations_CasualId",
                table: "AdobeConnectOperations");

            migrationBuilder.DropIndex(
                name: "IX_AdobeConnectOperations_CoverId",
                table: "AdobeConnectOperations");

            migrationBuilder.DropColumn(
                name: "TeacherMSTeamOperation_CoverId",
                table: "MSTeamOperations");

            migrationBuilder.Sql("delete from MSTeamOperations", true);

            migrationBuilder.DropColumn(
                name: "CoverId",
                table: "MSTeamOperations");

            migrationBuilder.AddColumn<Guid>(
                name: "CoverId",
                table: "MSTeamOperations",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.DropColumn(
                name: "CasualId",
                table: "MSTeamOperations");

            migrationBuilder.AddColumn<Guid>(
                name: "CasualId",
                table: "MSTeamOperations",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCovered",
                table: "ClassworkNotifications",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql("delete from AdobeConnectOperations", true);

            migrationBuilder.DropColumn(
                name: "CoverId",
                table: "AdobeConnectOperations");

            migrationBuilder.AddColumn<Guid>(
                name: "CoverId",
                table: "AdobeConnectOperations",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.DropColumn(
                name: "CasualId",
                table: "AdobeConnectOperations");

            migrationBuilder.AddColumn<Guid>(
                name: "CasualId",
                table: "AdobeConnectOperations",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Casuals_Casuals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmailAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AdobeConnectId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SchoolCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_Casuals_Casuals", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Covers_ClassCovers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OfferingId = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TeacherType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TeacherId = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_Covers_ClassCovers", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Casuals_Casuals");

            migrationBuilder.DropTable(
                name: "Covers_ClassCovers");

            migrationBuilder.DropColumn(
                name: "IsCovered",
                table: "ClassworkNotifications");

            migrationBuilder.AlterColumn<int>(
                name: "CoverId",
                table: "MSTeamOperations",
                type: "int",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<int>(
                name: "CasualId",
                table: "MSTeamOperations",
                type: "int",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TeacherMSTeamOperation_CoverId",
                table: "MSTeamOperations",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CoverId",
                table: "AdobeConnectOperations",
                type: "int",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<int>(
                name: "CasualId",
                table: "AdobeConnectOperations",
                type: "int",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "Casuals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SchoolCode = table.Column<string>(type: "nvarchar(4)", nullable: true),
                    AdobeConnectPrincipalId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateDeleted = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateEntered = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PortalUsername = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Casuals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Casuals_Schools_SchoolCode",
                        column: x => x.SchoolCode,
                        principalTable: "Schools",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateTable(
                name: "Covers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OfferingId = table.Column<int>(type: "int", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CasualId = table.Column<int>(type: "int", nullable: true),
                    StaffId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Covers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Covers_Casuals_CasualId",
                        column: x => x.CasualId,
                        principalTable: "Casuals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Covers_Offerings_OfferingId",
                        column: x => x.OfferingId,
                        principalTable: "Offerings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Covers_Staff_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Staff",
                        principalColumn: "StaffId");
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

            migrationBuilder.CreateIndex(
                name: "IX_MSTeamOperations_CasualId",
                table: "MSTeamOperations",
                column: "CasualId");

            migrationBuilder.CreateIndex(
                name: "IX_MSTeamOperations_CoverId",
                table: "MSTeamOperations",
                column: "CoverId");

            migrationBuilder.CreateIndex(
                name: "IX_MSTeamOperations_TeacherMSTeamOperation_CoverId",
                table: "MSTeamOperations",
                column: "TeacherMSTeamOperation_CoverId");

            migrationBuilder.CreateIndex(
                name: "IX_AdobeConnectOperations_CasualId",
                table: "AdobeConnectOperations",
                column: "CasualId");

            migrationBuilder.CreateIndex(
                name: "IX_AdobeConnectOperations_CoverId",
                table: "AdobeConnectOperations",
                column: "CoverId");

            migrationBuilder.CreateIndex(
                name: "IX_Casuals_SchoolCode",
                table: "Casuals",
                column: "SchoolCode");

            migrationBuilder.CreateIndex(
                name: "IX_ClassCoverClassworkNotification_CoversId",
                table: "ClassCoverClassworkNotification",
                column: "CoversId");

            migrationBuilder.CreateIndex(
                name: "IX_Covers_CasualId",
                table: "Covers",
                column: "CasualId");

            migrationBuilder.CreateIndex(
                name: "IX_Covers_OfferingId",
                table: "Covers",
                column: "OfferingId");

            migrationBuilder.CreateIndex(
                name: "IX_Covers_StaffId",
                table: "Covers",
                column: "StaffId");

            migrationBuilder.AddForeignKey(
                name: "FK_AdobeConnectOperations_Casuals_CasualId",
                table: "AdobeConnectOperations",
                column: "CasualId",
                principalTable: "Casuals",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AdobeConnectOperations_Covers_CoverId",
                table: "AdobeConnectOperations",
                column: "CoverId",
                principalTable: "Covers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MSTeamOperations_Casuals_CasualId",
                table: "MSTeamOperations",
                column: "CasualId",
                principalTable: "Casuals",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MSTeamOperations_Covers_CoverId",
                table: "MSTeamOperations",
                column: "CoverId",
                principalTable: "Covers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MSTeamOperations_Covers_TeacherMSTeamOperation_CoverId",
                table: "MSTeamOperations",
                column: "TeacherMSTeamOperation_CoverId",
                principalTable: "Covers",
                principalColumn: "Id");
        }
    }
}
