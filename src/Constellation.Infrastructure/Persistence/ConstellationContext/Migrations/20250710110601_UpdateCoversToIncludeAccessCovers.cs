using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCoversToIncludeAccessCovers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Covers_ClassCovers_Offerings_Offerings_OfferingId",
                table: "Covers_ClassCovers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Covers_ClassCovers",
                table: "Covers_ClassCovers");

            migrationBuilder.EnsureSchema(
                name: "Covers");

            migrationBuilder.RenameTable(
                name: "Covers_ClassCovers",
                newName: "Covers",
                newSchema: "Covers");

            migrationBuilder.RenameIndex(
                name: "IX_Covers_ClassCovers_OfferingId",
                schema: "Covers",
                table: "Covers",
                newName: "IX_Covers_OfferingId");

            migrationBuilder.AddColumn<string>(
                name: "CoverType",
                schema: "Covers",
                table: "Covers",
                type: "nvarchar(13)",
                maxLength: 13,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(@"
                UPDATE [Covers].[Covers]
                SET CoverType = 'ClassCover'
                WHERE 1 = 1;");

            migrationBuilder.AddColumn<string>(
                name: "Note",
                schema: "Covers",
                table: "Covers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Covers",
                schema: "Covers",
                table: "Covers",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Covers_Offerings_Offerings_OfferingId",
                schema: "Covers",
                table: "Covers",
                column: "OfferingId",
                principalTable: "Offerings_Offerings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Covers_Offerings_Offerings_OfferingId",
                schema: "Covers",
                table: "Covers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Covers",
                schema: "Covers",
                table: "Covers");

            migrationBuilder.DropColumn(
                name: "CoverType",
                schema: "Covers",
                table: "Covers");

            migrationBuilder.DropColumn(
                name: "Note",
                schema: "Covers",
                table: "Covers");

            migrationBuilder.RenameTable(
                name: "Covers",
                schema: "Covers",
                newName: "Covers_ClassCovers");

            migrationBuilder.RenameIndex(
                name: "IX_Covers_OfferingId",
                table: "Covers_ClassCovers",
                newName: "IX_Covers_ClassCovers_OfferingId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Covers_ClassCovers",
                table: "Covers_ClassCovers",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Covers_ClassCovers_Offerings_Offerings_OfferingId",
                table: "Covers_ClassCovers",
                column: "OfferingId",
                principalTable: "Offerings_Offerings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
