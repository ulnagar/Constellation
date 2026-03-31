using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    /// <inheritdoc />
    public partial class RefactorMessageQueueToProvideLogsToSender : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Error",
                schema: "Messages",
                table: "Queue");

            migrationBuilder.AddColumn<string>(
                name: "Errors",
                schema: "Messages",
                table: "Queue",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "HasErrors",
                schema: "Messages",
                table: "Queue",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                schema: "Messages",
                table: "Queue",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Errors",
                schema: "Messages",
                table: "Queue");

            migrationBuilder.DropColumn(
                name: "HasErrors",
                schema: "Messages",
                table: "Queue");

            migrationBuilder.DropColumn(
                name: "UserId",
                schema: "Messages",
                table: "Queue");

            migrationBuilder.AddColumn<string>(
                name: "Error",
                schema: "Messages",
                table: "Queue",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
