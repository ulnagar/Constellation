using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    /// <inheritdoc />
    public partial class ConvertParentInterviewToAttendeeList : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ParentId",
                table: "WorkFlows_Actions");

            migrationBuilder.CreateTable(
                name: "WorkFlows_Actions_InterviewAttendees",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ActionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkFlows_Actions_InterviewAttendees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkFlows_Actions_InterviewAttendees_WorkFlows_Actions_ActionId",
                        column: x => x.ActionId,
                        principalTable: "WorkFlows_Actions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkFlows_Actions_InterviewAttendees_ActionId",
                table: "WorkFlows_Actions_InterviewAttendees",
                column: "ActionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkFlows_Actions_InterviewAttendees");

            migrationBuilder.AddColumn<Guid>(
                name: "ParentId",
                table: "WorkFlows_Actions",
                type: "uniqueidentifier",
                nullable: true);
        }
    }
}
