using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    /// <inheritdoc />
    public partial class AllowNullsForSapEquipmentNumberField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Assets_SapEquipmentNumber",
                schema: "Assets",
                table: "Assets");

            migrationBuilder.AlterColumn<string>(
                name: "SapEquipmentNumber",
                schema: "Assets",
                table: "Assets",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_SapEquipmentNumber",
                schema: "Assets",
                table: "Assets",
                column: "SapEquipmentNumber",
                unique: true,
                filter: "[SapEquipmentNumber] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Assets_SapEquipmentNumber",
                schema: "Assets",
                table: "Assets");

            migrationBuilder.AlterColumn<string>(
                name: "SapEquipmentNumber",
                schema: "Assets",
                table: "Assets",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Assets_SapEquipmentNumber",
                schema: "Assets",
                table: "Assets",
                column: "SapEquipmentNumber",
                unique: true);
        }
    }
}
