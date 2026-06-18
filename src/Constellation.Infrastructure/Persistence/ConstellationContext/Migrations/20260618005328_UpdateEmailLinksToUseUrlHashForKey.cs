using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEmailLinksToUseUrlHashForKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_EmailLinks",
                schema: "Messages",
                table: "EmailLinks");

            migrationBuilder.AddColumn<byte[]>(
                name: "UrlHash",
                schema: "Messages",
                table: "EmailLinks",
                type: "binary(32)",
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE Messages.EmailLinks
                SET UrlHash = HASHBYTES('SHA2_256', CAST(DestinationUrl AS NVARCHAR(MAX)));",
                suppressTransaction: true);

            migrationBuilder.Sql(@"
        ALTER TABLE [Messages].[EmailLinks] ALTER COLUMN [UrlHash] binary(32) NOT NULL
    ", suppressTransaction: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_EmailLinks",
                schema: "Messages",
                table: "EmailLinks",
                columns: new[] { "EmailId", "UrlHash" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_EmailLinks",
                schema: "Messages",
                table: "EmailLinks");

            migrationBuilder.DropColumn(
                name: "UrlHash",
                schema: "Messages",
                table: "EmailLinks");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EmailLinks",
                schema: "Messages",
                table: "EmailLinks",
                columns: new[] { "EmailId", "DestinationUrl" });
        }
    }
}
