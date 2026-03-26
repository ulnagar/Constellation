using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailLinkTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_EmailTrackingEvents_EmailMessageId_EventType",
                schema: "Messages",
                table: "EmailTrackingEvents",
                newName: "IX_EmailTrackingEvents_EmailId_EventType");

            migrationBuilder.RenameIndex(
                name: "IX_Messages_EmailRecipients_EmailId_RecipientType",
                schema: "Messages",
                table: "EmailRecipients",
                newName: "IX_EmailRecipients_EmailId_RecipientType");

            migrationBuilder.RenameIndex(
                name: "IX_Messages_EmailRecipients_Email",
                schema: "Messages",
                table: "EmailRecipients",
                newName: "IX_EmailRecipients_Email");

            migrationBuilder.RenameIndex(
                name: "IX_Messages_Email_Status",
                schema: "Messages",
                table: "Email",
                newName: "IX_Email_Status");

            migrationBuilder.RenameIndex(
                name: "IX_Messages_Email_SentAt",
                schema: "Messages",
                table: "Email",
                newName: "IX_Email_SentAt");

            migrationBuilder.RenameIndex(
                name: "IX_Messages_Email_ProviderMessageId",
                schema: "Messages",
                table: "Email",
                newName: "IX_Email_ProviderMessageId");

            migrationBuilder.AddColumn<int>(
                name: "ClickCount",
                schema: "Messages",
                table: "Email",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "FirstClickedAt",
                schema: "Messages",
                table: "Email",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastClickedAt",
                schema: "Messages",
                table: "Email",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EmailLinks",
                schema: "Messages",
                columns: table => new
                {
                    EmailId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DestinationUrl = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    ClickCount = table.Column<int>(type: "int", nullable: false),
                    FirstClickedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LastClickedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailLinks", x => new { x.EmailId, x.DestinationUrl });
                    table.ForeignKey(
                        name: "FK_EmailLinks_Email_EmailId",
                        column: x => x.EmailId,
                        principalSchema: "Messages",
                        principalTable: "Email",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmailLinks_EmailId",
                schema: "Messages",
                table: "EmailLinks",
                column: "EmailId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmailLinks",
                schema: "Messages");

            migrationBuilder.DropColumn(
                name: "ClickCount",
                schema: "Messages",
                table: "Email");

            migrationBuilder.DropColumn(
                name: "FirstClickedAt",
                schema: "Messages",
                table: "Email");

            migrationBuilder.DropColumn(
                name: "LastClickedAt",
                schema: "Messages",
                table: "Email");

            migrationBuilder.RenameIndex(
                name: "IX_EmailTrackingEvents_EmailId_EventType",
                schema: "Messages",
                table: "EmailTrackingEvents",
                newName: "IX_EmailTrackingEvents_EmailMessageId_EventType");

            migrationBuilder.RenameIndex(
                name: "IX_EmailRecipients_EmailId_RecipientType",
                schema: "Messages",
                table: "EmailRecipients",
                newName: "IX_Messages_EmailRecipients_EmailId_RecipientType");

            migrationBuilder.RenameIndex(
                name: "IX_EmailRecipients_Email",
                schema: "Messages",
                table: "EmailRecipients",
                newName: "IX_Messages_EmailRecipients_Email");

            migrationBuilder.RenameIndex(
                name: "IX_Email_Status",
                schema: "Messages",
                table: "Email",
                newName: "IX_Messages_Email_Status");

            migrationBuilder.RenameIndex(
                name: "IX_Email_SentAt",
                schema: "Messages",
                table: "Email",
                newName: "IX_Messages_Email_SentAt");

            migrationBuilder.RenameIndex(
                name: "IX_Email_ProviderMessageId",
                schema: "Messages",
                table: "Email",
                newName: "IX_Messages_Email_ProviderMessageId");
        }
    }
}
