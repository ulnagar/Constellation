using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    /// <inheritdoc />
    public partial class IncludeComplianceWorkFlowCase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "WorkFlows_CaseDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedById",
                table: "WorkFlows_CaseDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "WorkFlows_CaseDetails",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IncidentId",
                table: "WorkFlows_CaseDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IncidentType",
                table: "WorkFlows_CaseDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Subject",
                table: "WorkFlows_CaseDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "MarkedNotCompleted",
                table: "WorkFlows_Actions",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "MarkedResolved",
                table: "WorkFlows_Actions",
                type: "bit",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "WorkFlows_CaseDetails");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "WorkFlows_CaseDetails");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "WorkFlows_CaseDetails");

            migrationBuilder.DropColumn(
                name: "IncidentId",
                table: "WorkFlows_CaseDetails");

            migrationBuilder.DropColumn(
                name: "IncidentType",
                table: "WorkFlows_CaseDetails");

            migrationBuilder.DropColumn(
                name: "Subject",
                table: "WorkFlows_CaseDetails");

            migrationBuilder.DropColumn(
                name: "MarkedNotCompleted",
                table: "WorkFlows_Actions");

            migrationBuilder.DropColumn(
                name: "MarkedResolved",
                table: "WorkFlows_Actions");
        }
    }
}
