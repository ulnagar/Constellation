using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCanvasOperationsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Students_Schools_SchoolCode",
                table: "Students");

            migrationBuilder.DropTable(
                name: "Setting");

            migrationBuilder.DropIndex(
                name: "IX_Students_SchoolCode",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "SchoolCode",
                schema: "Students",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "UpdateUserEmailCanvasOperation_PortalUsername",
                table: "CanvasOperations");

            migrationBuilder.DropTable(
                name: "Students");

            migrationBuilder.DropTable(
                name: "Students_AbsenceConfiguration");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SchoolCode",
                schema: "Students",
                table: "Students",
                type: "nvarchar(4)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdateUserEmailCanvasOperation_PortalUsername",
                table: "CanvasOperations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Setting",
                columns: table => new
                {
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Setting", x => new { x.Name, x.Type });
                });

            migrationBuilder.CreateIndex(
                name: "IX_Students_SchoolCode",
                schema: "Students",
                table: "Students",
                column: "SchoolCode");

            migrationBuilder.AddForeignKey(
                name: "FK_Students_Schools_SchoolCode",
                schema: "Students",
                table: "Students",
                column: "SchoolCode",
                principalTable: "Schools",
                principalColumn: "Code");
        }
    }
}
