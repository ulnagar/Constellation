using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    /// <inheritdoc />
    public partial class AddThirdPartyConsent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ThirdParty_Applications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Purpose = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InformationCollected = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StoredCountry = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SharedWith = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConsentRequired = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThirdParty_Applications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ThirdParty_Transactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SubmittedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SubmissionMethod = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SubmissionNotes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThirdParty_Transactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ThirdParty_Transactions_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "StudentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ThirdParty_Consents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ConsentProvided = table.Column<bool>(type: "bit", nullable: false),
                    ProvidedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProvidedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Method = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MethodNotes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThirdParty_Consents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ThirdParty_Consents_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "StudentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ThirdParty_Consents_ThirdParty_Applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "ThirdParty_Applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ThirdParty_Consents_ThirdParty_Transactions_TransactionId",
                        column: x => x.TransactionId,
                        principalTable: "ThirdParty_Transactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ThirdParty_Consents_ApplicationId",
                table: "ThirdParty_Consents",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_ThirdParty_Consents_StudentId",
                table: "ThirdParty_Consents",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_ThirdParty_Consents_TransactionId",
                table: "ThirdParty_Consents",
                column: "TransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_ThirdParty_Transactions_StudentId",
                table: "ThirdParty_Transactions",
                column: "StudentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ThirdParty_Consents");

            migrationBuilder.DropTable(
                name: "ThirdParty_Applications");

            migrationBuilder.DropTable(
                name: "ThirdParty_Transactions");
        }
    }
}
