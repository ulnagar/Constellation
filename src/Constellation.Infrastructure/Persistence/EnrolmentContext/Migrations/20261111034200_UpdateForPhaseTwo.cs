using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Constellation.Infrastructure.Persistence.EnrolmentContext.Migrations
{
    /// <inheritdoc />
    public partial class UpdateForPhaseTwo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "ParentEmailAddress",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "ParentPhoneNumber",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "Parent_FirstName",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "Parent_LastName",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "Parent_PreferredName",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "Postcode",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "State",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "Street",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "StudentEmailAddress",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "StudentGender",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "StudentReferenceNumber",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "Student_FirstName",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "Student_LastName",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "Student_PreferredName",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "Town",
                table: "Applications");

            migrationBuilder.AddColumn<Guid>(
                name: "RespondedBy",
                table: "Offers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "StudentId",
                table: "Applications",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "Parents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EnrolmentUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Street = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Town = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    State = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    Postcode = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: true),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PreferredName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Parents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Students",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentReferenceNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Gender = table.Column<string>(type: "nvarchar(3)", nullable: false),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PreferredName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Students", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationEnrolmentParent",
                columns: table => new
                {
                    ApplicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParentsId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationEnrolmentParent", x => new { x.ApplicationId, x.ParentsId });
                    table.ForeignKey(
                        name: "FK_ApplicationEnrolmentParent_Applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "Applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApplicationEnrolmentParent_Parents_ParentsId",
                        column: x => x.ParentsId,
                        principalTable: "Parents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EnrolmentParentEnrolmentStudent",
                columns: table => new
                {
                    ParentsId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentsId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnrolmentParentEnrolmentStudent", x => new { x.ParentsId, x.StudentsId });
                    table.ForeignKey(
                        name: "FK_EnrolmentParentEnrolmentStudent_Parents_ParentsId",
                        column: x => x.ParentsId,
                        principalTable: "Parents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EnrolmentParentEnrolmentStudent_Students_StudentsId",
                        column: x => x.StudentsId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Applications_StudentId",
                table: "Applications",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationEnrolmentParent_ParentsId",
                table: "ApplicationEnrolmentParent",
                column: "ParentsId");

            migrationBuilder.CreateIndex(
                name: "IX_EnrolmentParentEnrolmentStudent_StudentsId",
                table: "EnrolmentParentEnrolmentStudent",
                column: "StudentsId");

            migrationBuilder.AddForeignKey(
                name: "FK_Applications_Students_StudentId",
                table: "Applications",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Applications_Students_StudentId",
                table: "Applications");

            migrationBuilder.DropTable(
                name: "ApplicationEnrolmentParent");

            migrationBuilder.DropTable(
                name: "EnrolmentParentEnrolmentStudent");

            migrationBuilder.DropTable(
                name: "Parents");

            migrationBuilder.DropTable(
                name: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Applications_StudentId",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "RespondedBy",
                table: "Offers");

            migrationBuilder.DropColumn(
                name: "StudentId",
                table: "Applications");

            migrationBuilder.AddColumn<DateOnly>(
                name: "DateOfBirth",
                table: "Applications",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ParentEmailAddress",
                table: "Applications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ParentPhoneNumber",
                table: "Applications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Parent_FirstName",
                table: "Applications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Parent_LastName",
                table: "Applications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Parent_PreferredName",
                table: "Applications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Postcode",
                table: "Applications",
                type: "nvarchar(4)",
                maxLength: 4,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "State",
                table: "Applications",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Street",
                table: "Applications",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StudentEmailAddress",
                table: "Applications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StudentGender",
                table: "Applications",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "StudentReferenceNumber",
                table: "Applications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Student_FirstName",
                table: "Applications",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Student_LastName",
                table: "Applications",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Student_PreferredName",
                table: "Applications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Town",
                table: "Applications",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }
    }
}
