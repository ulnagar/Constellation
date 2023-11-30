using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    /// <inheritdoc />
    public partial class UpdateFacultyToAggregate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MSTeamOperations_Faculties_FacultyId",
                table: "MSTeamOperations");

            migrationBuilder.DropForeignKey(
                name: "FK_Subjects_Courses_Faculties_FacultyId",
                table: "Subjects_Courses");

            migrationBuilder.DropForeignKey(
                name: "FK_FacultyMembership_Faculties_FacultyId",
                table: "FacultyMembership");

            migrationBuilder.DropForeignKey(
                name: "FK_FacultyMembership_Staff_StaffId",
                table: "FacultyMembership");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Faculties",
                table: "Faculties");

            migrationBuilder.RenameTable(
                name: "Faculties",
                newName: "Faculties_Faculty");

            migrationBuilder.AlterColumn<Guid>(
                name: "FacultyId",
                table: "Subjects_Courses",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Faculties_Faculty",
                table: "Faculties_Faculty",
                column: "Id");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FacultyMembership",
                table: "FacultyMembership");

            migrationBuilder.RenameTable(
                name: "FacultyMembership",
                newName: "Faculties_Memberships");

            migrationBuilder.AlterColumn<Guid>(
                name: "FacultyId",
                table: "Faculties_Memberships",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.RenameColumn(
                name: "Role",
                table: "Faculties_Memberships",
                newName: "xRole");
            
            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "Faculties_Memberships",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.Sql(
                @"UPDATE [dbo].[Faculties_Memberships]
                    SET [Role] = 'Member'
                    WHERE [xRole] = 0;");

            migrationBuilder.Sql(
                @"UPDATE [dbo].[Faculties_Memberships]
                    SET [Role] = 'Approver'
                    WHERE [xRole] = 1;");

            migrationBuilder.Sql(
                @"UPDATE [dbo].[Faculties_Memberships]
                    SET [Role] = 'Manager'
                    WHERE [xRole] = 2;");

            migrationBuilder.DropColumn(
                name: "xRole",
                table: "Faculties_Memberships");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Faculties_Memberships",
                table: "Faculties_Memberships",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Faculties_Memberships_Faculties_Faculty_FacultyId",
                table: "Faculties_Memberships",
                column: "FacultyId",
                principalTable: "Faculties_Faculty",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Faculties_Memberships_Staff_StaffId",
                table: "Faculties_Memberships",
                column: "StaffId",
                principalTable: "Staff",
                principalColumn: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_Faculties_Memberships_FacultyId",
                table: "Faculties_Memberships",
                column: "FacultyId");

            migrationBuilder.CreateIndex(
                name: "IX_Faculties_Memberships_StaffId",
                table: "Faculties_Memberships",
                column: "StaffId");

            migrationBuilder.AddForeignKey(
                name: "FK_MSTeamOperations_Faculties_Faculty_FacultyId",
                table: "MSTeamOperations",
                column: "FacultyId",
                principalTable: "Faculties_Faculty",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Subjects_Courses_Faculties_Faculty_FacultyId",
                table: "Subjects_Courses",
                column: "FacultyId",
                principalTable: "Faculties_Faculty",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MSTeamOperations_Faculties_Faculty_FacultyId",
                table: "MSTeamOperations");

            migrationBuilder.DropForeignKey(
                name: "FK_Subjects_Courses_Faculties_Faculty_FacultyId",
                table: "Subjects_Courses");

            migrationBuilder.DropTable(
                name: "Faculties_Memberships");

            migrationBuilder.DropTable(
                name: "Faculties_Faculty");

            migrationBuilder.AlterColumn<Guid>(
                name: "FacultyId",
                table: "Subjects_Courses",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "Faculties",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Colour = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Faculties", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FacultyMembership",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FacultyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StaffId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Role = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FacultyMembership", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FacultyMembership_Faculties_FacultyId",
                        column: x => x.FacultyId,
                        principalTable: "Faculties",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FacultyMembership_Staff_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Staff",
                        principalColumn: "StaffId");
                });

            migrationBuilder.InsertData(
                table: "Faculties",
                columns: new[] { "Id", "Colour", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "IsDeleted", "ModifiedAt", "ModifiedBy", "Name" },
                values: new object[,]
                {
                    { new Guid("30cfc9b6-662a-11ed-9022-0242ac120002"), "", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, false, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Administration" },
                    { new Guid("30cfce98-662a-11ed-9022-0242ac120002"), "", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, false, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Executive" },
                    { new Guid("30cfd05a-662a-11ed-9022-0242ac120002"), "", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, false, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "English" },
                    { new Guid("30cfd26c-662a-11ed-9022-0242ac120002"), "", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, false, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Mathematics" },
                    { new Guid("30cfd3de-662a-11ed-9022-0242ac120002"), "", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, false, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Science" },
                    { new Guid("30cfd51e-662a-11ed-9022-0242ac120002"), "", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, false, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Stage 3" },
                    { new Guid("30cfda8c-662a-11ed-9022-0242ac120002"), "", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, false, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Support" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_FacultyMembership_FacultyId",
                table: "FacultyMembership",
                column: "FacultyId");

            migrationBuilder.CreateIndex(
                name: "IX_FacultyMembership_StaffId",
                table: "FacultyMembership",
                column: "StaffId");

            migrationBuilder.AddForeignKey(
                name: "FK_MSTeamOperations_Faculties_FacultyId",
                table: "MSTeamOperations",
                column: "FacultyId",
                principalTable: "Faculties",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Subjects_Courses_Faculties_FacultyId",
                table: "Subjects_Courses",
                column: "FacultyId",
                principalTable: "Faculties",
                principalColumn: "Id");
        }
    }
}
