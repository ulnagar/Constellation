using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    /// <inheritdoc />
    public partial class AddConsentRequirements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ThirdParty_Consents_Students_StudentId",
                table: "ThirdParty_Consents");

            migrationBuilder.DropForeignKey(
                name: "FK_ThirdParty_Consents_ThirdParty_Applications_ApplicationId",
                table: "ThirdParty_Consents");

            migrationBuilder.DropForeignKey(
                name: "FK_ThirdParty_Consents_ThirdParty_Transactions_TransactionId",
                table: "ThirdParty_Consents");

            migrationBuilder.DropForeignKey(
                name: "FK_ThirdParty_Transactions_Students_StudentId",
                table: "ThirdParty_Transactions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ThirdParty_Transactions",
                table: "ThirdParty_Transactions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ThirdParty_Consents",
                table: "ThirdParty_Consents");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ThirdParty_Applications",
                table: "ThirdParty_Applications");

            migrationBuilder.EnsureSchema(
                name: "ThirdParty");

            migrationBuilder.RenameTable(
                name: "ThirdParty_Transactions",
                newName: "Transactions",
                newSchema: "ThirdParty");

            migrationBuilder.RenameTable(
                name: "ThirdParty_Consents",
                newName: "Consents",
                newSchema: "ThirdParty");

            migrationBuilder.RenameTable(
                name: "ThirdParty_Applications",
                newName: "Applications",
                newSchema: "ThirdParty");

            migrationBuilder.RenameIndex(
                name: "IX_ThirdParty_Transactions_StudentId",
                schema: "ThirdParty",
                table: "Transactions",
                newName: "IX_Transactions_StudentId");

            migrationBuilder.RenameIndex(
                name: "IX_ThirdParty_Consents_TransactionId",
                schema: "ThirdParty",
                table: "Consents",
                newName: "IX_Consents_TransactionId");

            migrationBuilder.RenameIndex(
                name: "IX_ThirdParty_Consents_StudentId",
                schema: "ThirdParty",
                table: "Consents",
                newName: "IX_Consents_StudentId");

            migrationBuilder.RenameIndex(
                name: "IX_ThirdParty_Consents_ApplicationId",
                schema: "ThirdParty",
                table: "Consents",
                newName: "IX_Consents_ApplicationId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Transactions",
                schema: "ThirdParty",
                table: "Transactions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Consents",
                schema: "ThirdParty",
                table: "Consents",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Applications",
                schema: "ThirdParty",
                table: "Applications",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "ConsentRequirements",
                schema: "ThirdParty",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(21)", maxLength: 21, nullable: false),
                    CourseId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Grade = table.Column<int>(type: "int", nullable: true),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsentRequirements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConsentRequirements_Applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalSchema: "ThirdParty",
                        principalTable: "Applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConsentRequirements_Students_StudentId",
                        column: x => x.StudentId,
                        principalSchema: "Students",
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConsentRequirements_Subjects_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Subjects_Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConsentRequirements_ApplicationId",
                schema: "ThirdParty",
                table: "ConsentRequirements",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsentRequirements_CourseId",
                schema: "ThirdParty",
                table: "ConsentRequirements",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsentRequirements_StudentId",
                schema: "ThirdParty",
                table: "ConsentRequirements",
                column: "StudentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Consents_Applications_ApplicationId",
                schema: "ThirdParty",
                table: "Consents",
                column: "ApplicationId",
                principalSchema: "ThirdParty",
                principalTable: "Applications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Consents_Students_StudentId",
                schema: "ThirdParty",
                table: "Consents",
                column: "StudentId",
                principalSchema: "Students",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Consents_Transactions_TransactionId",
                schema: "ThirdParty",
                table: "Consents",
                column: "TransactionId",
                principalSchema: "ThirdParty",
                principalTable: "Transactions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Students_StudentId",
                schema: "ThirdParty",
                table: "Transactions",
                column: "StudentId",
                principalSchema: "Students",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Consents_Applications_ApplicationId",
                schema: "ThirdParty",
                table: "Consents");

            migrationBuilder.DropForeignKey(
                name: "FK_Consents_Students_StudentId",
                schema: "ThirdParty",
                table: "Consents");

            migrationBuilder.DropForeignKey(
                name: "FK_Consents_Transactions_TransactionId",
                schema: "ThirdParty",
                table: "Consents");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Students_StudentId",
                schema: "ThirdParty",
                table: "Transactions");

            migrationBuilder.DropTable(
                name: "ConsentRequirements",
                schema: "ThirdParty");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Transactions",
                schema: "ThirdParty",
                table: "Transactions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Consents",
                schema: "ThirdParty",
                table: "Consents");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Applications",
                schema: "ThirdParty",
                table: "Applications");

            migrationBuilder.RenameTable(
                name: "Transactions",
                schema: "ThirdParty",
                newName: "ThirdParty_Transactions");

            migrationBuilder.RenameTable(
                name: "Consents",
                schema: "ThirdParty",
                newName: "ThirdParty_Consents");

            migrationBuilder.RenameTable(
                name: "Applications",
                schema: "ThirdParty",
                newName: "ThirdParty_Applications");

            migrationBuilder.RenameIndex(
                name: "IX_Transactions_StudentId",
                table: "ThirdParty_Transactions",
                newName: "IX_ThirdParty_Transactions_StudentId");

            migrationBuilder.RenameIndex(
                name: "IX_Consents_TransactionId",
                table: "ThirdParty_Consents",
                newName: "IX_ThirdParty_Consents_TransactionId");

            migrationBuilder.RenameIndex(
                name: "IX_Consents_StudentId",
                table: "ThirdParty_Consents",
                newName: "IX_ThirdParty_Consents_StudentId");

            migrationBuilder.RenameIndex(
                name: "IX_Consents_ApplicationId",
                table: "ThirdParty_Consents",
                newName: "IX_ThirdParty_Consents_ApplicationId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ThirdParty_Transactions",
                table: "ThirdParty_Transactions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ThirdParty_Consents",
                table: "ThirdParty_Consents",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ThirdParty_Applications",
                table: "ThirdParty_Applications",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ThirdParty_Consents_Students_StudentId",
                table: "ThirdParty_Consents",
                column: "StudentId",
                principalSchema: "Students",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ThirdParty_Consents_ThirdParty_Applications_ApplicationId",
                table: "ThirdParty_Consents",
                column: "ApplicationId",
                principalTable: "ThirdParty_Applications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ThirdParty_Consents_ThirdParty_Transactions_TransactionId",
                table: "ThirdParty_Consents",
                column: "TransactionId",
                principalTable: "ThirdParty_Transactions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ThirdParty_Transactions_Students_StudentId",
                table: "ThirdParty_Transactions",
                column: "StudentId",
                principalSchema: "Students",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
