using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    /// <inheritdoc />
    public partial class StartTrackingFullSenderAndReceiverInformationForSms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Sms_From_To_CreatedAt",
                schema: "Messages",
                table: "Sms");

            migrationBuilder.AddColumn<string>(
                name: "Recipient_Name",
                schema: "Messages",
                table: "Sms",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Recipient_Number",
                schema: "Messages",
                table: "Sms",
                type: "nvarchar(320)",
                maxLength: 320,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Sender_Name",
                schema: "Messages",
                table: "Sms",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Sender_Number",
                schema: "Messages",
                table: "Sms",
                type: "nvarchar(320)",
                maxLength: 320,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(
                @"UPDATE Messages.Sms
                    SET Recipient_Number = [To],
                        Recipient_Name = [To],
                        Sender_Number = [From],
                        Sender_Name = [From]
                    WHERE 1=1;");

            migrationBuilder.DropColumn(
                name: "From",
                schema: "Messages",
                table: "Sms");

            migrationBuilder.DropColumn(
                name: "StatusUpdatedAt",
                schema: "Messages",
                table: "Sms");

            migrationBuilder.DropColumn(
                name: "To",
                schema: "Messages",
                table: "Sms");
            
            migrationBuilder.CreateIndex(
                name: "IX_Sms_Recipient_Number",
                schema: "Messages",
                table: "Sms",
                column: "Recipient_Number");

            migrationBuilder.CreateIndex(
                name: "IX_Sms_Sender_Number",
                schema: "Messages",
                table: "Sms",
                column: "Sender_Number");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Sms_Recipient_Number",
                schema: "Messages",
                table: "Sms");

            migrationBuilder.DropIndex(
                name: "IX_Sms_Sender_Number",
                schema: "Messages",
                table: "Sms");

            migrationBuilder.DropColumn(
                name: "Recipient_Name",
                schema: "Messages",
                table: "Sms");

            migrationBuilder.DropColumn(
                name: "Recipient_Number",
                schema: "Messages",
                table: "Sms");

            migrationBuilder.DropColumn(
                name: "Sender_Name",
                schema: "Messages",
                table: "Sms");

            migrationBuilder.DropColumn(
                name: "Sender_Number",
                schema: "Messages",
                table: "Sms");

            migrationBuilder.AddColumn<string>(
                name: "From",
                schema: "Messages",
                table: "Sms",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "StatusUpdatedAt",
                schema: "Messages",
                table: "Sms",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "To",
                schema: "Messages",
                table: "Sms",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Sms_From_To_CreatedAt",
                schema: "Messages",
                table: "Sms",
                columns: new[] { "From", "To", "CreatedAt" });
        }
    }
}
