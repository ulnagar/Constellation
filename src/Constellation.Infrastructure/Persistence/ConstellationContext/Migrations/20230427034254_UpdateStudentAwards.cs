using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    /// <inheritdoc />
    public partial class UpdateStudentAwards : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StudentAward");

            migrationBuilder.CreateTable(
                name: "Awards_StudentAwards",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    TeacherId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    AwardedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IncidentId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Awards_StudentAwards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Awards_StudentAwards_Staff_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "Staff",
                        principalColumn: "StaffId");
                    table.ForeignKey(
                        name: "FK_Awards_StudentAwards_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "StudentId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Awards_StudentAwards_IncidentId",
                table: "Awards_StudentAwards",
                column: "IncidentId");

            migrationBuilder.CreateIndex(
                name: "IX_Awards_StudentAwards_StudentId",
                table: "Awards_StudentAwards",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Awards_StudentAwards_TeacherId",
                table: "Awards_StudentAwards",
                column: "TeacherId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Awards_StudentAwards");

            migrationBuilder.CreateTable(
                name: "StudentAward",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    AwardedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentAward", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentAward_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "StudentId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_StudentAward_StudentId",
                table: "StudentAward",
                column: "StudentId");
        }
    }
}
