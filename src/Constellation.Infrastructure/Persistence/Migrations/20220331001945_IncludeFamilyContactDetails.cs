using Microsoft.EntityFrameworkCore.Migrations;

namespace Constellation.Infrastructure.Persistence.Migrations
{
    public partial class IncludeFamilyContactDetails : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FamilyId",
                table: "Students",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "StudentFamilies",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Parent1_Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Parent1_FirstName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Parent1_LastName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Parent1_MobileNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Parent1_EmailAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Parent2_Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Parent2_FirstName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Parent2_LastName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Parent2_MobileNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Parent2_EmailAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address_Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address_Line1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address_Line2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address_Town = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address_PostCode = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentFamilies", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Students_FamilyId",
                table: "Students",
                column: "FamilyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Students_StudentFamilies_FamilyId",
                table: "Students",
                column: "FamilyId",
                principalTable: "StudentFamilies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Students_StudentFamilies_FamilyId",
                table: "Students");

            migrationBuilder.DropTable(
                name: "StudentFamilies");

            migrationBuilder.DropIndex(
                name: "IX_Students_FamilyId",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "FamilyId",
                table: "Students");
        }
    }
}
