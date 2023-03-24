using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    /// <inheritdoc />
    public partial class UpdateForStronglyTypedIdsInEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MandatoryTraining_CompletionRecords_MandatoryTraining_Modules_TrainingModuleId",
                table: "MandatoryTraining_CompletionRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_MandatoryTraining_CompletionRecords_Staff_StaffId",
                table: "MandatoryTraining_CompletionRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_MandatoryTraining_CompletionRecords_StoredFiles_StoredFileId",
                table: "MandatoryTraining_CompletionRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_Students_StudentFamilies_FamilyId",
                table: "Students");

            migrationBuilder.DropTable(
                name: "StudentFamilies");

            migrationBuilder.DropIndex(
                name: "IX_Students_FamilyId",
                table: "Students");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MandatoryTraining_CompletionRecords",
                table: "MandatoryTraining_CompletionRecords");

            migrationBuilder.DropColumn(
                name: "FamilyId",
                table: "Students");

            migrationBuilder.RenameTable(
                name: "MandatoryTraining_CompletionRecords",
                newName: "MandatoryTraining_Completions");

            migrationBuilder.RenameIndex(
                name: "IX_MandatoryTraining_CompletionRecords_TrainingModuleId",
                table: "MandatoryTraining_Completions",
                newName: "IX_MandatoryTraining_Completions_TrainingModuleId");

            migrationBuilder.RenameIndex(
                name: "IX_MandatoryTraining_CompletionRecords_StoredFileId",
                table: "MandatoryTraining_Completions",
                newName: "IX_MandatoryTraining_Completions_StoredFileId");

            migrationBuilder.RenameIndex(
                name: "IX_MandatoryTraining_CompletionRecords_StaffId",
                table: "MandatoryTraining_Completions",
                newName: "IX_MandatoryTraining_Completions_StaffId");

            migrationBuilder.AlterColumn<Guid>(
                name: "TutorialId",
                table: "GroupTutorials_Teachers",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "TutorialId",
                table: "GroupTutorials_Roll",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "TutorialId",
                table: "GroupTutorials_Enrolment",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "SchoolCode",
                table: "Casuals_Casuals",
                type: "nvarchar(4)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EmailAddress",
                table: "Casuals_Casuals",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "TrainingModuleId",
                table: "MandatoryTraining_Completions",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MandatoryTraining_Completions",
                table: "MandatoryTraining_Completions",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Families_Family",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SentralId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FamilyTitle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AddressLine1 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AddressLine2 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AddressTown = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AddressPostCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FamilyEmail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Families_Family", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Families_Parents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FamilyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MobileNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmailAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SentralLink = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Families_Parents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Families_Parents_Families_Family_FamilyId",
                        column: x => x.FamilyId,
                        principalTable: "Families_Family",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Families_StudentMemberships",
                columns: table => new
                {
                    StudentId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FamilyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsResidentialFamily = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Families_StudentMemberships", x => new { x.FamilyId, x.StudentId });
                    table.ForeignKey(
                        name: "FK_Families_StudentMemberships_Families_Family_FamilyId",
                        column: x => x.FamilyId,
                        principalTable: "Families_Family",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Families_StudentMemberships_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "StudentId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Covers_ClassCovers_OfferingId",
                table: "Covers_ClassCovers",
                column: "OfferingId");

            migrationBuilder.CreateIndex(
                name: "IX_Casuals_Casuals_EmailAddress",
                table: "Casuals_Casuals",
                column: "EmailAddress",
                unique: true,
                filter: "[EmailAddress] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Casuals_Casuals_SchoolCode",
                table: "Casuals_Casuals",
                column: "SchoolCode");

            migrationBuilder.CreateIndex(
                name: "IX_Families_Parents_FamilyId",
                table: "Families_Parents",
                column: "FamilyId");

            migrationBuilder.CreateIndex(
                name: "IX_Families_StudentMemberships_StudentId",
                table: "Families_StudentMemberships",
                column: "StudentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Casuals_Casuals_Schools_SchoolCode",
                table: "Casuals_Casuals",
                column: "SchoolCode",
                principalTable: "Schools",
                principalColumn: "Code",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Covers_ClassCovers_Offerings_OfferingId",
                table: "Covers_ClassCovers",
                column: "OfferingId",
                principalTable: "Offerings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MandatoryTraining_Completions_MandatoryTraining_Modules_TrainingModuleId",
                table: "MandatoryTraining_Completions",
                column: "TrainingModuleId",
                principalTable: "MandatoryTraining_Modules",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MandatoryTraining_Completions_Staff_StaffId",
                table: "MandatoryTraining_Completions",
                column: "StaffId",
                principalTable: "Staff",
                principalColumn: "StaffId");

            migrationBuilder.AddForeignKey(
                name: "FK_MandatoryTraining_Completions_StoredFiles_StoredFileId",
                table: "MandatoryTraining_Completions",
                column: "StoredFileId",
                principalTable: "StoredFiles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Casuals_Casuals_Schools_SchoolCode",
                table: "Casuals_Casuals");

            migrationBuilder.DropForeignKey(
                name: "FK_Covers_ClassCovers_Offerings_OfferingId",
                table: "Covers_ClassCovers");

            migrationBuilder.DropForeignKey(
                name: "FK_MandatoryTraining_Completions_MandatoryTraining_Modules_TrainingModuleId",
                table: "MandatoryTraining_Completions");

            migrationBuilder.DropForeignKey(
                name: "FK_MandatoryTraining_Completions_Staff_StaffId",
                table: "MandatoryTraining_Completions");

            migrationBuilder.DropForeignKey(
                name: "FK_MandatoryTraining_Completions_StoredFiles_StoredFileId",
                table: "MandatoryTraining_Completions");

            migrationBuilder.DropTable(
                name: "Families_Parents");

            migrationBuilder.DropTable(
                name: "Families_StudentMemberships");

            migrationBuilder.DropTable(
                name: "Families_Family");

            migrationBuilder.DropIndex(
                name: "IX_Covers_ClassCovers_OfferingId",
                table: "Covers_ClassCovers");

            migrationBuilder.DropIndex(
                name: "IX_Casuals_Casuals_EmailAddress",
                table: "Casuals_Casuals");

            migrationBuilder.DropIndex(
                name: "IX_Casuals_Casuals_SchoolCode",
                table: "Casuals_Casuals");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MandatoryTraining_Completions",
                table: "MandatoryTraining_Completions");

            migrationBuilder.RenameTable(
                name: "MandatoryTraining_Completions",
                newName: "MandatoryTraining_CompletionRecords");

            migrationBuilder.RenameIndex(
                name: "IX_MandatoryTraining_Completions_TrainingModuleId",
                table: "MandatoryTraining_CompletionRecords",
                newName: "IX_MandatoryTraining_CompletionRecords_TrainingModuleId");

            migrationBuilder.RenameIndex(
                name: "IX_MandatoryTraining_Completions_StoredFileId",
                table: "MandatoryTraining_CompletionRecords",
                newName: "IX_MandatoryTraining_CompletionRecords_StoredFileId");

            migrationBuilder.RenameIndex(
                name: "IX_MandatoryTraining_Completions_StaffId",
                table: "MandatoryTraining_CompletionRecords",
                newName: "IX_MandatoryTraining_CompletionRecords_StaffId");

            migrationBuilder.AddColumn<string>(
                name: "FamilyId",
                table: "Students",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "TutorialId",
                table: "GroupTutorials_Teachers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "TutorialId",
                table: "GroupTutorials_Roll",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "TutorialId",
                table: "GroupTutorials_Enrolment",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SchoolCode",
                table: "Casuals_Casuals",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(4)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EmailAddress",
                table: "Casuals_Casuals",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "TrainingModuleId",
                table: "MandatoryTraining_CompletionRecords",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_MandatoryTraining_CompletionRecords",
                table: "MandatoryTraining_CompletionRecords",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "StudentFamilies",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EmailAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address_Line1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address_Line2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address_PostCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address_Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address_Town = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Parent1_EmailAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Parent1_FirstName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Parent1_LastName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Parent1_MobileNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Parent1_Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Parent2_EmailAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Parent2_FirstName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Parent2_LastName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Parent2_MobileNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Parent2_Title = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentFamilies", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Students_FamilyId",
                table: "Students",
                column: "FamilyId");

            migrationBuilder.AddForeignKey(
                name: "FK_MandatoryTraining_CompletionRecords_MandatoryTraining_Modules_TrainingModuleId",
                table: "MandatoryTraining_CompletionRecords",
                column: "TrainingModuleId",
                principalTable: "MandatoryTraining_Modules",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MandatoryTraining_CompletionRecords_Staff_StaffId",
                table: "MandatoryTraining_CompletionRecords",
                column: "StaffId",
                principalTable: "Staff",
                principalColumn: "StaffId");

            migrationBuilder.AddForeignKey(
                name: "FK_MandatoryTraining_CompletionRecords_StoredFiles_StoredFileId",
                table: "MandatoryTraining_CompletionRecords",
                column: "StoredFileId",
                principalTable: "StoredFiles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Students_StudentFamilies_FamilyId",
                table: "Students",
                column: "FamilyId",
                principalTable: "StudentFamilies",
                principalColumn: "Id");
        }
    }
}
