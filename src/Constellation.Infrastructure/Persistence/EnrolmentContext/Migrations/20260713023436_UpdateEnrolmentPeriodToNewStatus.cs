using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Constellation.Infrastructure.Persistence.EnrolmentContext.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEnrolmentPeriodToNewStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Periods");

            migrationBuilder.AddColumn<bool>(
                name: "IsSuspended",
                table: "Periods",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SuspensionReason",
                table: "Periods",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSuspended",
                table: "Periods");

            migrationBuilder.DropColumn(
                name: "SuspensionReason",
                table: "Periods");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Periods",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
