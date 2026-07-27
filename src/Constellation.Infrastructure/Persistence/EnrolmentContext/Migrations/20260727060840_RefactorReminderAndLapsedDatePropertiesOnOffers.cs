using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Constellation.Infrastructure.Persistence.EnrolmentContext.Migrations
{
    /// <inheritdoc />
    public partial class RefactorReminderAndLapsedDatePropertiesOnOffers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RespondBy",
                table: "Offers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "RespondBy",
                table: "Offers",
                type: "datetimeoffset",
                nullable: true);
        }
    }
}
