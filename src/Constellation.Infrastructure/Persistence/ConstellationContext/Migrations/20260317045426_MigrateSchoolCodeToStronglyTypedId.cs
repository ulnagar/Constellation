using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    /// <inheritdoc />
    public partial class MigrateSchoolCodeToStronglyTypedId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SchoolContactRole_SchoolCode",
                table: "SchoolContacts_Roles");

            migrationBuilder.DropForeignKey(
                name: "FK_Rolls_Schools_SchoolCode",
                schema: "SciencePracs",
                table: "Rolls");

            migrationBuilder.AlterColumn<string>(
                name: "SchoolCode",
                table: "SchoolContacts_Roles",
                type: "nvarchar(4)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(4)",
                oldNullable: true);

            migrationBuilder.Sql(
                @"DELETE FROM SciencePracs.Rolls
                    WHERE SchoolCode is null;");

            migrationBuilder.AlterColumn<string>(
                name: "SchoolCode",
                schema: "SciencePracs",
                table: "Rolls",
                type: "nvarchar(4)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(4)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Rolls_Schools_SchoolCode",
                schema: "SciencePracs",
                table: "Rolls",
                column: "SchoolCode",
                principalTable: "Schools",
                principalColumn: "Code",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rolls_Schools_SchoolCode",
                schema: "SciencePracs",
                table: "Rolls");

            migrationBuilder.AlterColumn<string>(
                name: "SchoolCode",
                table: "SchoolContacts_Roles",
                type: "nvarchar(4)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(4)");

            migrationBuilder.AlterColumn<string>(
                name: "SchoolCode",
                schema: "SciencePracs",
                table: "Rolls",
                type: "nvarchar(4)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(4)");

            migrationBuilder.AddForeignKey(
                name: "FK_Rolls_Schools_SchoolCode",
                schema: "SciencePracs",
                table: "Rolls",
                column: "SchoolCode",
                principalTable: "Schools",
                principalColumn: "Code");
        }
    }
}
