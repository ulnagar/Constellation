using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    /// <inheritdoc />
    public partial class AddSmsMessages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Messages");

            migrationBuilder.CreateTable(
                name: "Sms",
                schema: "Messages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SmsGlobalId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    OutgoingId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    From = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    To = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(1600)", maxLength: 1600, nullable: false),
                    Direction = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    SmsGlobalDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    StatusUpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ReplyToId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sms_Sms_ReplyToId",
                        column: x => x.ReplyToId,
                        principalSchema: "Messages",
                        principalTable: "Sms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Sms_From_To_CreatedAt",
                schema: "Messages",
                table: "Sms",
                columns: new[] { "From", "To", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Sms_OutgoingId",
                schema: "Messages",
                table: "Sms",
                column: "OutgoingId");

            migrationBuilder.CreateIndex(
                name: "IX_Sms_ReplyToId",
                schema: "Messages",
                table: "Sms",
                column: "ReplyToId");

            migrationBuilder.CreateIndex(
                name: "IX_Sms_SmsGlobalId",
                schema: "Messages",
                table: "Sms",
                column: "SmsGlobalId");

            migrationBuilder.CreateIndex(
                name: "IX_Sms_Status_Direction",
                schema: "Messages",
                table: "Sms",
                columns: new[] { "Status", "Direction" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Sms",
                schema: "Messages");
        }
    }
}
