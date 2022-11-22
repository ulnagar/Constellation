using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    public partial class AllowNotRequiredResponsesToTrainingModules : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "CanMarkNotRequired",
                table: "MandatoryTraining_Modules",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CompletedDate",
                table: "MandatoryTraining_CompletionRecords",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<bool>(
                name: "NotRequired",
                table: "MandatoryTraining_CompletionRecords",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CanMarkNotRequired",
                table: "MandatoryTraining_Modules");

            migrationBuilder.DropColumn(
                name: "NotRequired",
                table: "MandatoryTraining_CompletionRecords");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CompletedDate",
                table: "MandatoryTraining_CompletionRecords",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);
        }
    }
}
