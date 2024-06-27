using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkflowForTraining : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DueDate",
                table: "WorkFlows_CaseDetails",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModuleName",
                table: "WorkFlows_CaseDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StaffId",
                table: "WorkFlows_CaseDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TrainingModuleId",
                table: "WorkFlows_CaseDetails",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModuleName",
                table: "WorkFlows_Actions",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "WorkFlows_CaseDetails");

            migrationBuilder.DropColumn(
                name: "ModuleName",
                table: "WorkFlows_CaseDetails");

            migrationBuilder.DropColumn(
                name: "StaffId",
                table: "WorkFlows_CaseDetails");

            migrationBuilder.DropColumn(
                name: "TrainingModuleId",
                table: "WorkFlows_CaseDetails");

            migrationBuilder.DropColumn(
                name: "ModuleName",
                table: "WorkFlows_Actions");
        }
    }
}
