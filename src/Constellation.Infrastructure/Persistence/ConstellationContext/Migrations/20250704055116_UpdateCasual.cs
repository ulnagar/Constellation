using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCasual : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DisplayName",
                table: "Casuals_Casuals");

            migrationBuilder.AddColumn<string>(
                name: "PreferredName",
                table: "Casuals_Casuals",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.RenameColumn(
                name: "AdobeConnectId",
                table: "Casuals_Casuals",
                newName: "EdvalTeacherId");

            migrationBuilder.Sql(@"
                UPDATE [dbo].[Casuals_Casuals]
                SET EdvalTeacherId = null
                WHERE 1 = 1;");

            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                table: "Casuals_Casuals",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FirstName",
                table: "Casuals_Casuals",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PreferredName",
                table: "Casuals_Casuals",
                newName: "DisplayName");

            migrationBuilder.RenameColumn(
                name: "EdvalTeacherId",
                table: "Casuals_Casuals",
                newName: "AdobeConnectId");

            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                table: "Casuals_Casuals",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "FirstName",
                table: "Casuals_Casuals",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }
    }
}
