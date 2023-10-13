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
                name: "FK_MandatoryTraining_Completions_StoredFiles_StoredFileId",
                table: "MandatoryTraining_Completions");

            migrationBuilder.DropIndex(
                name: "IX_MandatoryTraining_Completions_StoredFileId",
                table: "MandatoryTraining_Completions");

            migrationBuilder.DropColumn(
                name: "StoredFileId",
                table: "MandatoryTraining_Completions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StoredFiles",
                table: "StoredFiles");

            migrationBuilder.RenameTable(
                name: "StoredFiles",
                newName: "Attachments_Attachments");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Attachments_Attachments",
                newName: "xId");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "Attachments_Attachments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql(
                @"UPDATE [dbo].[Attachments_Attachments]
                    SET [Id] = NEWID()
                    WHERE 1 = 1;");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Attachments_Attachments",
                nullable: false);

            migrationBuilder.DropColumn(
                name: "xId",
                table: "Attachments_Attachments");

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "Attachments_Attachments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FileSize",
                table: "Attachments_Attachments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Attachments_Attachments",
                nullable: false,
                defaultValueSql: "GETDATE()");

            migrationBuilder.AlterColumn<string>(
                name: "LinkType",
                table: "Attachments_Attachments",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "LinkId",
                table: "Attachments_Attachments",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Attachments_Attachments",
                table: "Attachments_Attachments",
                column: "Id");
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
