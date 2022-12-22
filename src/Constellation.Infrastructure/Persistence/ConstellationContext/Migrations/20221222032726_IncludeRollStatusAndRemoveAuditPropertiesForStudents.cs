using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    public partial class IncludeRollStatusAndRemoveAuditPropertiesForStudents : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "GroupTutorials_RollStudent");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "GroupTutorials_RollStudent");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "GroupTutorials_RollStudent");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "GroupTutorials_RollStudent");

            migrationBuilder.DropColumn(
                name: "ModifiedAt",
                table: "GroupTutorials_RollStudent");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "GroupTutorials_RollStudent");

            migrationBuilder.RenameColumn(
                name: "IsDeleted",
                table: "GroupTutorials_RollStudent",
                newName: "Enrolled");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Enrolled",
                table: "GroupTutorials_RollStudent",
                newName: "IsDeleted");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "GroupTutorials_RollStudent",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "GroupTutorials_RollStudent",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "GroupTutorials_RollStudent",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "GroupTutorials_RollStudent",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedAt",
                table: "GroupTutorials_RollStudent",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "GroupTutorials_RollStudent",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
