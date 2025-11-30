using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    /// <inheritdoc />
    public partial class MigrateTeamOperationsToSharedId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence<int>(
                name: "TeamsOperationId",
                startValue: 202500L);

            migrationBuilder
                .DropPrimaryKey(
                    name: "PK_Teams",
                    schema: "Operations",
                    table: "Teams");

            migrationBuilder
                .DropColumn(
                    name: "Id",
                    schema: "Operations",
                    table: "Teams");

            migrationBuilder
                .AddColumn<int>(
                    name: "Id",
                    schema: "Operations",
                    table: "Teams",
                    type: "int",
                    nullable: false,
                    defaultValueSql: "NEXT VALUE FOR TeamsOperationId");

            migrationBuilder
                .AddPrimaryKey(
                    name: "PK_Teams",
                    schema: "Operations",
                    table: "Teams",
                    column: "Id");

            migrationBuilder
                .DropPrimaryKey(
                    name: "PK_MSTeamOperations",
                    table: "MSTeamOperations");

            migrationBuilder
                .DropColumn(
                    name: "Id",
                    table: "MSTeamOperations");

            migrationBuilder
                .AddColumn<int>(
                    name: "Id",
                    table: "MSTeamOperations",
                    type: "int",
                    nullable: false,
                    defaultValueSql: "NEXT VALUE FOR TeamsOperationId");

            migrationBuilder
                .AddPrimaryKey(
                    name: "PK_MSTeamOperations",
                    table: "MSTeamOperations",
                    column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropSequence(
                name: "TeamsOperationId");

            migrationBuilder
                .DropPrimaryKey(
                    name: "PK_Teams",
                    schema: "Operations",
                    table: "Teams");

            migrationBuilder
                .DropColumn(
                    name: "Id",
                    schema: "Operations",
                    table: "Teams");

            migrationBuilder
                .AddColumn<int>(
                    name: "Id",
                    schema: "Operations",
                    table: "Teams",
                    type: "int",
                    nullable: false)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder
                .AddPrimaryKey(
                    name: "PK_Teams",
                    schema: "Operations",
                    table: "Teams",
                    column: "Id");

            migrationBuilder
                .DropPrimaryKey(
                    name: "PK_MSTeamOperations",
                    table: "MSTeamOperations");

            migrationBuilder
                .DropColumn(
                    name: "Id",
                    table: "MSTeamOperations");

            migrationBuilder
                .AddColumn<int>(
                    name: "Id",
                    table: "MSTeamOperations",
                    type: "int",
                    nullable: false)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder
                .AddPrimaryKey(
                    name: "PK_MSTeamOperations",
                    table: "MSTeamOperations",
                    column: "Id");
        }
    }
}
