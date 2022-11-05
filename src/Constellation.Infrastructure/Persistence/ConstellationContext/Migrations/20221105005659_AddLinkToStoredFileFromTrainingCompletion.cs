using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    public partial class AddLinkToStoredFileFromTrainingCompletion : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StoredFileId",
                table: "MandatoryTraining_CompletionRecords",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MandatoryTraining_CompletionRecords_StoredFileId",
                table: "MandatoryTraining_CompletionRecords",
                column: "StoredFileId");

            migrationBuilder.AddForeignKey(
                name: "FK_MandatoryTraining_CompletionRecords_StoredFiles_StoredFileId",
                table: "MandatoryTraining_CompletionRecords",
                column: "StoredFileId",
                principalTable: "StoredFiles",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MandatoryTraining_CompletionRecords_StoredFiles_StoredFileId",
                table: "MandatoryTraining_CompletionRecords");

            migrationBuilder.DropIndex(
                name: "IX_MandatoryTraining_CompletionRecords_StoredFileId",
                table: "MandatoryTraining_CompletionRecords");

            migrationBuilder.DropColumn(
                name: "StoredFileId",
                table: "MandatoryTraining_CompletionRecords");
        }
    }
}
