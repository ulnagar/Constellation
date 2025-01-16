using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    using System.Xml.Schema;

    /// <inheritdoc />
    public partial class MigrateToNewPeriodAggregate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Timetables");

            migrationBuilder.RenameTable(
                name: "Periods",
                newName: "Periods",
                newSchema: "Timetables");

            migrationBuilder.DropIndex(
                name: "IX_Offerings_Sessions_PeriodId",
                table: "Offerings_Sessions");

            migrationBuilder.DropForeignKey(
                name: "FK_Offerings_Sessions_Periods_PeriodId",
                table: "Offerings_Sessions");

            migrationBuilder.RenameColumn(
                name: "PeriodId",
                table: "Offerings_Sessions",
                newName: "xPeriodId");

            migrationBuilder.AddColumn<Guid>(
                name: "PeriodId",
                table: "Offerings_Sessions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.DropPrimaryKey(
                name: "PK_Periods",
                table: "Periods",
                schema: "Timetables");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Periods",
                schema: "Timetables",
                newName: "xId");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "Periods",
                schema: "Timetables",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql(
                @"UPDATE [Timetables].[Periods]
                    SET [Id] = NEWID()
                    WHERE 1 = 1;");

            migrationBuilder.Sql(
                @"UPDATE [dbo].[Offerings_Sessions]
                 SET [PeriodId] = [Timetables].[Periods].[Id]
                 FROM [Timetables].[Periods]
                 INNER JOIN [dbo].[Offerings_Sessions]
                 ON [dbo].[Offerings_Sessions].[xPeriodId] = [Timetables].[Periods].[xId];");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Periods",
                schema: "Timetables",
                nullable: false);

            migrationBuilder.DropColumn(
                name: "xId",
                table: "Periods",
                schema: "Timetables");

            migrationBuilder.AlterColumn<Guid>(
                name: "PeriodId",
                table: "Offerings_Sessions",
                nullable: false);

            migrationBuilder.DropColumn(
                name: "xPeriodId",
                table: "Offerings_Sessions");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Periods",
                table: "Periods",
                schema: "Timetables",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Offerings_Sessions_Periods_PeriodId",
                table: "Offerings_Sessions",
                principalTable: "Periods",
                principalSchema: "Timetables",
                principalColumn: "Id",
                column: "PeriodId");

            migrationBuilder.DropColumn(
                name: "Period",
                table: "Periods",
                schema: "Timetables");

            migrationBuilder.AddColumn<string>(
                name: "PeriodCode",
                schema: "Timetables",
                table: "Periods",
                type: "nvarchar(1)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(
                @"UPDATE [Timetables].[Periods]
                    SET [PeriodCode] = RIGHT([Name], 1)
                    WHERE Name like 'Period%';");

            migrationBuilder.Sql(
                @"UPDATE [Timetables].[Periods]
                    SET [PeriodCode] = UPPER(LEFT([Name], 1))
                    WHERE Name not like 'Period%';");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "Periods",
                schema: "Timetables",
                newName: "xType");

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Periods",
                schema: "Timetables",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.Sql(
                @"UPDATE [Timetables].[Periods]
                    SET [Type] = [xType];");

            migrationBuilder.Sql(
                @"UPDATE [Timetables].[Periods]
                    SET [Type] = 'Offline'
                    WHERE [Type] = 'Other';");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "Periods",
                schema: "Timetables",
                nullable: false,
                defaultValue: "");

            migrationBuilder.DropColumn(
                name: "xType",
                table: "Periods",
                schema: "Timetables");

            migrationBuilder.Sql(
                @"UPDATE [Timetables].[Periods]
                    SET [Timetable] = 'PRI'
                    WHERE [Timetable] = 'PRIMARY';");

            migrationBuilder.Sql(
                @"UPDATE [Timetables].[Periods]
                    SET [Timetable] = 'JU6'
                    WHERE [Timetable] = 'SECONDARY';");

            migrationBuilder.Sql(
                @"UPDATE [Timetables].[Periods]
                    SET [Timetable] = 'JU6'
                    WHERE [Timetable] is null;");

            migrationBuilder.AlterColumn<string>(
                name: "Timetable",
                schema: "Timetables",
                table: "Periods",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "Timetables",
                table: "Periods",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.RenameColumn(
                name: "Day",
                table: "Periods",
                schema: "Timetables",
                newName: "xDay");

            migrationBuilder.AddColumn<string>(
                name: "Day",
                table: "Periods",
                schema: "Timetables",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Week",
                table: "Periods",
                schema: "Timetables",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(
                @"UPDATE [Timetables].[Periods]
                    SET [Week] = 1
                    WHERE [xDay] < 6;");

            migrationBuilder.Sql(
                @"UPDATE [Timetables].[Periods]
                    SET [Week] = 2
                    WHERE [xDay] > 5;");

            migrationBuilder.Sql(
                @"UPDATE [Timetables].[Periods]
                    SET [Day] = 'Monday'
                    WHERE [xDay] = 1 or [xDay] = 6;");

            migrationBuilder.Sql(
                @"UPDATE [Timetables].[Periods]
                    SET [Day] = 'Tuesday'
                    WHERE [xDay] = 2 or [xDay] = 7;");

            migrationBuilder.Sql(
                @"UPDATE [Timetables].[Periods]
                    SET [Day] = 'Wednesday'
                    WHERE [xDay] = 3 or [xDay] = 8;");

            migrationBuilder.Sql(
                @"UPDATE [Timetables].[Periods]
                    SET [Day] = 'Thursday'
                    WHERE [xDay] = 4 or [xDay] = 9;");

            migrationBuilder.Sql(
                @"UPDATE [Timetables].[Periods]
                    SET [Day] = 'Friday'
                    WHERE [xDay] = 5 or [xDay] = 10;");

            migrationBuilder.AlterColumn<string>(
                name: "Day",
                table: "Periods",
                schema: "Timetables",
                nullable: false,
                defaultValue: "");

            migrationBuilder.DropColumn(
                name: "xDay",
                table: "Periods",
                schema: "Timetables");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                schema: "Timetables",
                table: "Periods",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                schema: "Timetables",
                table: "Periods",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.RenameColumn(
                name: "DateDeleted",
                table: "Periods",
                schema: "Timetables",
                newName: "DeletedAt");

            migrationBuilder.Sql(
                @"UPDATE [Timetables].[Periods]
                SET [DeletedAt] = Cast('0001-01-01' as datetime2)
                WHERE [DeletedAt] is null;");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DeletedAt",
                schema: "Timetables",
                table: "Periods",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
            
            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                schema: "Timetables",
                table: "Periods",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedAt",
                schema: "Timetables",
                table: "Periods",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                schema: "Timetables",
                table: "Periods",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                schema: "Timetables",
                table: "Periods");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "Timetables",
                table: "Periods");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                schema: "Timetables",
                table: "Periods");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                schema: "Timetables",
                table: "Periods");

            migrationBuilder.DropColumn(
                name: "ModifiedAt",
                schema: "Timetables",
                table: "Periods");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                schema: "Timetables",
                table: "Periods");

            migrationBuilder.DropColumn(
                name: "PeriodCode",
                schema: "Timetables",
                table: "Periods");

            migrationBuilder.RenameTable(
                name: "Periods",
                schema: "Timetables",
                newName: "Periods");

            migrationBuilder.RenameColumn(
                name: "Week",
                table: "Periods",
                newName: "Period");

            migrationBuilder.AlterColumn<int>(
                name: "PeriodId",
                table: "Offerings_Sessions",
                type: "int",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "Periods",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Timetable",
                table: "Periods",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Periods",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "Day",
                table: "Periods",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Periods",
                type: "int",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier")
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateDeleted",
                table: "Periods",
                type: "datetime2",
                nullable: true);
        }
    }
}
