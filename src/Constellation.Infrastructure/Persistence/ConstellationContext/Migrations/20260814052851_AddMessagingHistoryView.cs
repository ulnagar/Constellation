using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    /// <inheritdoc />
    public partial class AddMessagingHistoryView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                                 CREATE VIEW Messages.MessagingHistoryIndex AS
                                 SELECT
                                     e.Id,
                                     CAST('Email' AS varchar(10)) AS MessageType,
                                     e.CreatedAt AS CreatedAt,
                                     e.Subject AS Subject,
                                     e.From_Name AS FromName,
                                     e.From_Email AS FromAddress,
                                     e.BodyText as BodyText,
                                     (
                                         SELECT STRING_AGG(CONCAT(r.Name, N' ', r.Email), N' ')
                                         FROM Messages.EmailRecipients r
                                         WHERE r.EmailId = e.Id
                                     ) AS RecipientSearchText
                                 FROM Messages.Email e

                                 UNION ALL

                                 SELECT
                                     s.Id,
                                     CAST('Sms' AS varchar(10)) AS MessageType,
                                     s.CreatedAt AS CreatedAt,
                                     s.[Message] AS Subject,
                                     s.Sender_Name AS FromName,
                                     s.Sender_Number AS FromAddress,
                                     CAST(NULL as nvarchar(max)) AS BodyText,
                                     CONCAT(s.Recipient_Name, N' ', s.Recipient_Number) AS RecipientSearchText
                                 FROM Messages.Sms s;
                                 """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW Messages.MessagingHistoryIndex;");
        }
    }
}
