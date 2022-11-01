using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    public partial class ExtendAuditingPropertiesToCompletionRecords : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "MandatoryTraining_CompletionRecords",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "MandatoryTraining_CompletionRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "MandatoryTraining_CompletionRecords",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "MandatoryTraining_CompletionRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedAt",
                table: "MandatoryTraining_CompletionRecords",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "MandatoryTraining_CompletionRecords",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "MandatoryTraining_CompletionRecords");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "MandatoryTraining_CompletionRecords");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "MandatoryTraining_CompletionRecords");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "MandatoryTraining_CompletionRecords");

            migrationBuilder.DropColumn(
                name: "ModifiedAt",
                table: "MandatoryTraining_CompletionRecords");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "MandatoryTraining_CompletionRecords");
        }
    }
}
