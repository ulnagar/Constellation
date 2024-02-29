using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSchoolContactForChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MSTeamOperations_SchoolContact_ContactId",
                table: "MSTeamOperations");

            migrationBuilder.DropForeignKey(
                name: "FK_PartialAbsenceResponses_SchoolContact_VerifierId",
                table: "PartialAbsenceResponses");

            migrationBuilder.DropForeignKey(
                name: "FK_SciencePracs_Rolls_SchoolContact_SchoolContactId",
                table: "SciencePracs_Rolls");

            migrationBuilder.DropForeignKey(
                name: "FK_SchoolContactRole_SchoolContact_SchoolContactId",
                table: "SchoolContactRole");
            
            // Update entries that Fiona marked with her email (377 entries)
            migrationBuilder.Sql(@"
                UPDATE SciencePracs_Rolls
                SET SubmittedBy = 'fiona.boneham@det.nsw.edu.au'
                WHERE 
                    LessonDate is not null and
                    SchoolContactId is null and
                    SubmittedBy is null");

            // Update entries with the SchoolContactId set
            migrationBuilder.Sql(@"
                UPDATE SciencePracs_Rolls 
                SET SubmittedBy = C.EmailAddress
                FROM SciencePracs_Rolls R 
                    JOIN SchoolContact C on R.SchoolContactId = C.Id");

            // Update entries with the SubmittedBy set to name
            migrationBuilder.Sql(@"
                UPDATE SciencePracs_Rolls
                SET SubmittedBy = C.EmailAddress
                FROM SciencePracs_Rolls R join SChoolContact C on R.SubmittedBy = CONCAT(TRIM(C.FirstName), ' ', TRIM(C.LastName))");

            migrationBuilder.DropIndex(
                name: "IX_SciencePracs_Rolls_SchoolContactId",
                table: "SciencePracs_Rolls");

            migrationBuilder.DropColumn(
                name: "SchoolContactId",
                table: "SciencePracs_Rolls");

            // Update table for new ID Column
            migrationBuilder.DropPrimaryKey(
                name: "PK_SchoolContact",
                table: "SchoolContact");
            
            migrationBuilder.RenameColumn(
                name: "Id",
                table: "SchoolContact",
                newName: "OldId");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "SchoolContact",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            // Update PartialAbsenceResponses history table
            migrationBuilder.RenameColumn(
                name: "VerifierId",
                table: "PartialAbsenceResponses",
                newName: "OldVerifierId");
            
            migrationBuilder.AddColumn<Guid>(
                name: "VerifierId",
                table: "PartialAbsenceResponses",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE PartialAbsenceResponses 
                SET VerifierId = C.Id
                FROM PartialAbsenceResponses A 
                    JOIN SchoolContact C on A.OldVerifierId = C.OldId");

            migrationBuilder.DropColumn(
                name: "OldVerifierId",
                table: "PartialAbsenceResponses");

            // Update MSTeamOperations
            migrationBuilder.RenameColumn(
                name: "ContactId",
                table: "MSTeamOperations",
                newName: "OldContactId");

            migrationBuilder.AddColumn<Guid>(
                name: "ContactId",
                table: "MSTeamOperations",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE MSTeamOperations 
                SET ContactId = C.Id
                FROM MSTeamOperations O 
                    JOIN SchoolContact C on O.OldContactId = C.OldId");

            // Update User Tables
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AspNetUserTokens",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserTokens",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "ProviderKey",
                table: "AspNetUserLogins",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserLogins",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.RenameColumn(
                name: "SchoolContactId",
                table: "AspNetUsers",
                newName: "OldSchoolContactId");
            
            migrationBuilder.AddColumn<Guid>(
                name: "SchoolContactId",
                table: "AspNetUsers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE AspNetUsers 
                SET SchoolContactId = C.Id
                FROM AspNetUsers U 
                    JOIN SchoolContact C on U.OldSchoolContactId = C.OldId");

            //TODO: Modify table creation code to instead alter existing table. Perhaps move to top.

            migrationBuilder.CreateTable(
                name: "SchoolContacts_Contacts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmailAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SelfRegistered = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchoolContacts_Contacts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SchoolContacts_Roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SchoolContactId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SchoolCode = table.Column<string>(type: "nvarchar(4)", nullable: true),
                    SchoolName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_SchoolContacts_Roles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SchoolContacts_Roles_SchoolContacts_Contacts_SchoolContactId",
                        column: x => x.SchoolContactId,
                        principalTable: "SchoolContacts_Contacts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SchoolContacts_Roles_Schools_SchoolCode",
                        column: x => x.SchoolCode,
                        principalTable: "Schools",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SchoolContacts_Roles_SchoolCode",
                table: "SchoolContacts_Roles",
                column: "SchoolCode");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolContacts_Roles_SchoolContactId",
                table: "SchoolContacts_Roles",
                column: "SchoolContactId");

            migrationBuilder.AddForeignKey(
                name: "FK_MSTeamOperations_SchoolContacts_Contacts_ContactId",
                table: "MSTeamOperations",
                column: "ContactId",
                principalTable: "SchoolContacts_Contacts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PartialAbsenceResponses_SchoolContacts_Contacts_VerifierId",
                table: "PartialAbsenceResponses",
                column: "VerifierId",
                principalTable: "SchoolContacts_Contacts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MSTeamOperations_SchoolContacts_Contacts_ContactId",
                table: "MSTeamOperations");

            migrationBuilder.DropForeignKey(
                name: "FK_PartialAbsenceResponses_SchoolContacts_Contacts_VerifierId",
                table: "PartialAbsenceResponses");

            migrationBuilder.DropTable(
                name: "SchoolContacts_Roles");

            migrationBuilder.DropTable(
                name: "SchoolContacts_Contacts");

            migrationBuilder.DropColumn(
                name: "OldSchoolContactId",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<int>(
                name: "SchoolContactId",
                table: "SciencePracs_Rolls",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "VerifierId",
                table: "PartialAbsenceResponses",
                type: "int",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ContactId",
                table: "MSTeamOperations",
                type: "int",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AspNetUserTokens",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserTokens",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<int>(
                name: "SchoolContactId",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ProviderKey",
                table: "AspNetUserLogins",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserLogins",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.CreateTable(
                name: "SchoolContact",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DateDeleted = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateEntered = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EmailAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    SelfRegistered = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchoolContact", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SchoolContactRole",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SchoolCode = table.Column<string>(type: "nvarchar(4)", nullable: true),
                    SchoolContactId = table.Column<int>(type: "int", nullable: false),
                    DateDeleted = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateEntered = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchoolContactRole", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SchoolContactRole_SchoolContact_SchoolContactId",
                        column: x => x.SchoolContactId,
                        principalTable: "SchoolContact",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SchoolContactRole_Schools_SchoolCode",
                        column: x => x.SchoolCode,
                        principalTable: "Schools",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SciencePracs_Rolls_SchoolContactId",
                table: "SciencePracs_Rolls",
                column: "SchoolContactId");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolContactRole_SchoolCode",
                table: "SchoolContactRole",
                column: "SchoolCode");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolContactRole_SchoolContactId",
                table: "SchoolContactRole",
                column: "SchoolContactId");

            migrationBuilder.AddForeignKey(
                name: "FK_MSTeamOperations_SchoolContact_ContactId",
                table: "MSTeamOperations",
                column: "ContactId",
                principalTable: "SchoolContact",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PartialAbsenceResponses_SchoolContact_VerifierId",
                table: "PartialAbsenceResponses",
                column: "VerifierId",
                principalTable: "SchoolContact",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SciencePracs_Rolls_SchoolContact_SchoolContactId",
                table: "SciencePracs_Rolls",
                column: "SchoolContactId",
                principalTable: "SchoolContact",
                principalColumn: "Id");
        }
    }
}
