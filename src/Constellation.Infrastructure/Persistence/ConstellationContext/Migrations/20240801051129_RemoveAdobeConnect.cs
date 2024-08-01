using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    /// <inheritdoc />
    public partial class RemoveAdobeConnect : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdobeConnectOperations");

            migrationBuilder.DropTable(
                name: "Rooms");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Rooms",
                columns: table => new
                {
                    ScoId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DateDeleted = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Protected = table.Column<bool>(type: "bit", nullable: false),
                    UrlPath = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rooms", x => x.ScoId);
                });

            migrationBuilder.CreateTable(
                name: "AdobeConnectOperations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScoId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Action = table.Column<int>(type: "int", nullable: false),
                    CoverId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DateScheduled = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GroupSco = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    PrincipalId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserType = table.Column<string>(type: "nvarchar(21)", maxLength: 21, nullable: false),
                    CasualId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StudentId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    TeacherId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    GroupName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StaffId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdobeConnectOperations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdobeConnectOperations_Rooms_ScoId",
                        column: x => x.ScoId,
                        principalTable: "Rooms",
                        principalColumn: "ScoId");
                    table.ForeignKey(
                        name: "FK_AdobeConnectOperations_Staff_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Staff",
                        principalColumn: "StaffId");
                    table.ForeignKey(
                        name: "FK_AdobeConnectOperations_Staff_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "Staff",
                        principalColumn: "StaffId");
                    table.ForeignKey(
                        name: "FK_AdobeConnectOperations_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "StudentId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdobeConnectOperations_ScoId",
                table: "AdobeConnectOperations",
                column: "ScoId");

            migrationBuilder.CreateIndex(
                name: "IX_AdobeConnectOperations_StaffId",
                table: "AdobeConnectOperations",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_AdobeConnectOperations_StudentId",
                table: "AdobeConnectOperations",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_AdobeConnectOperations_TeacherId",
                table: "AdobeConnectOperations",
                column: "TeacherId");
        }
    }
}
