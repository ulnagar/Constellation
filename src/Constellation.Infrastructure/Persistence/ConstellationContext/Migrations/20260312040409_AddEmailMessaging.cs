using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailMessaging : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sms_Sms_ReplyToId",
                schema: "Messages",
                table: "Sms");

            migrationBuilder.DropIndex(
                name: "IX_Sms_ReplyToId",
                schema: "Messages",
                table: "Sms");

            migrationBuilder.DropColumn(
                name: "ReplyToId",
                schema: "Messages",
                table: "Sms");

            migrationBuilder.AddColumn<string>(
                name: "SendingModule",
                schema: "Messages",
                table: "Sms",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Email",
                schema: "Messages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SentAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    SendingModule = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    From_Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    From_Email = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: false),
                    ReplyTo_Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ReplyTo_Email = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: true),
                    Subject = table.Column<string>(type: "nvarchar(998)", maxLength: 998, nullable: false),
                    BodyText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BodyHtml = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Provider = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ProviderMessageId = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    OpenCount = table.Column<int>(type: "int", nullable: false),
                    FirstOpenedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LastOpenedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    TemplateId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Tags = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Email", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TrackingEventQueue",
                schema: "Automation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Payload = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EnqueuedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    Attempts = table.Column<int>(type: "int", nullable: false),
                    RetryAfter = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LastError = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackingEventQueue", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmailRecipients",
                schema: "Messages",
                columns: table => new
                {
                    EmailId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: false),
                    RecipientType = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailRecipients", x => new { x.EmailId, x.Email });
                    table.ForeignKey(
                        name: "FK_EmailRecipients_Email_EmailId",
                        column: x => x.EmailId,
                        principalSchema: "Messages",
                        principalTable: "Email",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmailTrackingEvents",
                schema: "Messages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmailId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    LinkUrl = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailTrackingEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailTrackingEvents_Email_EmailId",
                        column: x => x.EmailId,
                        principalSchema: "Messages",
                        principalTable: "Email",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Messages_Email_ProviderMessageId",
                schema: "Messages",
                table: "Email",
                column: "ProviderMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_Email_SentAt",
                schema: "Messages",
                table: "Email",
                column: "SentAt");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_Email_Status",
                schema: "Messages",
                table: "Email",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_EmailRecipients_Email",
                schema: "Messages",
                table: "EmailRecipients",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_EmailRecipients_EmailId_RecipientType",
                schema: "Messages",
                table: "EmailRecipients",
                columns: new[] { "EmailId", "RecipientType" });

            migrationBuilder.CreateIndex(
                name: "IX_EmailTrackingEvents_EmailMessageId_EventType",
                schema: "Messages",
                table: "EmailTrackingEvents",
                columns: new[] { "EmailId", "EventType" });

            migrationBuilder.CreateIndex(
                name: "IX_EmailTrackingEvents_OccurredAt",
                schema: "Messages",
                table: "EmailTrackingEvents",
                column: "OccurredAt");

            migrationBuilder.CreateIndex(
                name: "IX_Automation_TrackingEventQueue_RetryAfter_EnqueuedAt",
                schema: "Automation",
                table: "TrackingEventQueue",
                columns: new[] { "RetryAfter", "EnqueuedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmailRecipients",
                schema: "Messages");

            migrationBuilder.DropTable(
                name: "EmailTrackingEvents",
                schema: "Messages");

            migrationBuilder.DropTable(
                name: "TrackingEventQueue",
                schema: "Automation");

            migrationBuilder.DropTable(
                name: "Email",
                schema: "Messages");

            migrationBuilder.DropColumn(
                name: "SendingModule",
                schema: "Messages",
                table: "Sms");

            migrationBuilder.AddColumn<Guid>(
                name: "ReplyToId",
                schema: "Messages",
                table: "Sms",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sms_ReplyToId",
                schema: "Messages",
                table: "Sms",
                column: "ReplyToId");

            migrationBuilder.AddForeignKey(
                name: "FK_Sms_Sms_ReplyToId",
                schema: "Messages",
                table: "Sms",
                column: "ReplyToId",
                principalSchema: "Messages",
                principalTable: "Sms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
