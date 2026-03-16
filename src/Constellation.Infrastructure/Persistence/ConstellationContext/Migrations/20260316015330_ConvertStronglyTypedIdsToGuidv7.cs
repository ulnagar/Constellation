using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    /// <inheritdoc />
    public partial class ConvertStronglyTypedIdsToGuidv7 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"UPDATE WorkFlows_Cases
                    SET DetailId = '00000000-0000-0000-0000-000000000000'
                    WHERE DetailId IS NULL;");

            migrationBuilder.AlterColumn<Guid>(
                name: "DetailId",
                table: "WorkFlows_Cases",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.Sql(
                @"UPDATE WorkFlows_Actions_InterviewAttendees
                    SET ActionId = '00000000-0000-0000-0000-000000000000'
                    WHERE ActionId IS NULL;");

            migrationBuilder.AlterColumn<Guid>(
                name: "ActionId",
                table: "WorkFlows_Actions_InterviewAttendees",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.Sql(
                @"UPDATE WorkFlows_Actions
                    SET ParentActionId = '00000000-0000-0000-0000-000000000000'
                    WHERE ParentActionId IS NULL;");

            migrationBuilder.AlterColumn<Guid>(
                name: "ParentActionId",
                table: "WorkFlows_Actions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "DetailId",
                table: "WorkFlows_Cases",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "ActionId",
                table: "WorkFlows_Actions_InterviewAttendees",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "ParentActionId",
                table: "WorkFlows_Actions",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");
        }
    }
}
