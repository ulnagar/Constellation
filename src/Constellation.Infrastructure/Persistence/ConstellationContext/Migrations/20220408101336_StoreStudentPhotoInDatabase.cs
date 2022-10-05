using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    public partial class StoreStudentPhotoInDatabase : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "Photo",
                table: "Students",
                type: "varbinary(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Photo",
                table: "Students");
        }
    }
}
