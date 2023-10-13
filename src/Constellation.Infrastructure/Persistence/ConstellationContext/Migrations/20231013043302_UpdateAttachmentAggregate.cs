using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAttachmentAggregate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MandatoryTraining_Completions_Staff_StaffId",
                table: "MandatoryTraining_Completions");

            migrationBuilder.DropForeignKey(
                name: "FK_MandatoryTraining_Completions_StoredFiles_StoredFileId",
                table: "MandatoryTraining_Completions");

            migrationBuilder.DropTable(
                name: "StoredFiles");

            migrationBuilder.DropIndex(
                name: "IX_MandatoryTraining_Completions_StoredFileId",
                table: "MandatoryTraining_Completions");

            migrationBuilder.DropColumn(
                name: "StoredFileId",
                table: "MandatoryTraining_Completions");

            migrationBuilder.CreateTable(
                name: "Attachments_Attachments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FileType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FileData = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FileSize = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LinkType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LinkId = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attachments_Attachments", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_MandatoryTraining_Completions_Staff_StaffId",
                table: "MandatoryTraining_Completions",
                column: "StaffId",
                principalTable: "Staff",
                principalColumn: "StaffId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MandatoryTraining_Completions_Staff_StaffId",
                table: "MandatoryTraining_Completions");

            migrationBuilder.DropTable(
                name: "Attachments_Attachments");

            migrationBuilder.AddColumn<int>(
                name: "StoredFileId",
                table: "MandatoryTraining_Completions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "StoredFiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FileData = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    FileType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LinkId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LinkType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoredFiles", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MandatoryTraining_Completions_StoredFileId",
                table: "MandatoryTraining_Completions",
                column: "StoredFileId");

            migrationBuilder.AddForeignKey(
                name: "FK_MandatoryTraining_Completions_Staff_StaffId",
                table: "MandatoryTraining_Completions",
                column: "StaffId",
                principalTable: "Staff",
                principalColumn: "StaffId");

            migrationBuilder.AddForeignKey(
                name: "FK_MandatoryTraining_Completions_StoredFiles_StoredFileId",
                table: "MandatoryTraining_Completions",
                column: "StoredFileId",
                principalTable: "StoredFiles",
                principalColumn: "Id");
        }
    }
}
