using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    /// <inheritdoc />
    public partial class AddActionDescriptorToRequestNotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Id",
                schema: "Operations",
                table: "Teams",
                type: "int",
                nullable: false,
                defaultValueSql: "NEXT VALUE FOR [TeamsOperationId]",
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValueSql: "NEXT VALUE FOR TeamsOperationId");

            migrationBuilder.AddColumn<string>(
                name: "Action",
                schema: "Tutorials",
                table: "RequestNotes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "MSTeamOperations",
                type: "int",
                nullable: false,
                defaultValueSql: "NEXT VALUE FOR [TeamsOperationId]",
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValueSql: "NEXT VALUE FOR TeamsOperationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Action",
                schema: "Tutorials",
                table: "RequestNotes");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                schema: "Operations",
                table: "Teams",
                type: "int",
                nullable: false,
                defaultValueSql: "NEXT VALUE FOR TeamsOperationId",
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValueSql: "NEXT VALUE FOR [TeamsOperationId]");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "MSTeamOperations",
                type: "int",
                nullable: false,
                defaultValueSql: "NEXT VALUE FOR TeamsOperationId",
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValueSql: "NEXT VALUE FOR [TeamsOperationId]");
        }
    }
}
