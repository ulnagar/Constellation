using System;
using Constellation.Core.Models;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    public partial class ConvertToFacultyObject : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Faculties",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Colour = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_Faculties", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Faculties",
                columns: new[] { "Id", "Colour", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "IsDeleted", "ModifiedAt", "ModifiedBy", "Name" },
                values: new object[,]
                {
                    { new Guid("30cfc9b6-662a-11ed-9022-0242ac120002"), "", DateTime.Now, "System", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, false, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Administration" },
                    { new Guid("30cfce98-662a-11ed-9022-0242ac120002"), "", DateTime.Now, "System", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, false, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Executive" },
                    { new Guid("30cfd05a-662a-11ed-9022-0242ac120002"), "", DateTime.Now, "System", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, false, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "English" },
                    { new Guid("30cfd26c-662a-11ed-9022-0242ac120002"), "", DateTime.Now, "System", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, false, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Mathematics" },
                    { new Guid("30cfd3de-662a-11ed-9022-0242ac120002"), "", DateTime.Now, "System", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, false, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Science" },
                    { new Guid("30cfd51e-662a-11ed-9022-0242ac120002"), "", DateTime.Now, "System", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, false, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Stage 3" },
                    { new Guid("30cfda8c-662a-11ed-9022-0242ac120002"), "", DateTime.Now, "System", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, false, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Support" }
                });

            migrationBuilder.CreateTable(
                name: "FacultyMembership",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StaffId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    FacultyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false),
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

            migrationBuilder.AddColumn<Guid>(
                name: "FacultyId",
                table: "Courses",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            // Replace Course Faculty Flag "Admin" with new FacultyId
            //migrationBuilder.UpdateData(
            //    table: "Courses",
            //    keyColumns: new string[] { "Faculty" },
            //    keyValues: new object[] { 1, 2, 4, 8, 16, 32, 64 },
            //    keyColumnTypes: new string[] { typeof(int).ToString() },
            //    columns: new string[] { "FacultyId" },
            //    columnTypes: new string[] { typeof(Guid).ToString() },
            //    values: new object[] {
            //        new Guid("30cfc9b6-662a-11ed-9022-0242ac120002"),
            //        new Guid("30cfce98-662a-11ed-9022-0242ac120002"),
            //        new Guid("30cfd05a-662a-11ed-9022-0242ac120002"),
            //        new Guid("30cfd26c-662a-11ed-9022-0242ac120002"),
            //        new Guid("30cfd3de-662a-11ed-9022-0242ac120002"),
            //        new Guid("30cfda8c-662a-11ed-9022-0242ac120002"),
            //        new Guid("30cfd51e-662a-11ed-9022-0242ac120002") });

            migrationBuilder.UpdateData(
                table: "Courses",
                keyColumns: new string[] { "Faculty" },
                keyValues: new object[] { 1 },
                keyColumnTypes: new string[] { typeof(int).ToString() },
                columns: new string[] { "FacultyId" },
                columnTypes: new string[] { typeof(Guid).ToString() },
                values: new object[] {
                    new Guid("30cfc9b6-662a-11ed-9022-0242ac120002")});

            migrationBuilder.UpdateData(
                table: "Courses",
                keyColumns: new string[] { "Faculty" },
                keyValues: new object[] { 2 },
                keyColumnTypes: new string[] { typeof(int).ToString() },
                columns: new string[] { "FacultyId" },
                columnTypes: new string[] { typeof(Guid).ToString() },
                values: new object[] {
                    new Guid("30cfce98-662a-11ed-9022-0242ac120002")});

            migrationBuilder.UpdateData(
                table: "Courses",
                keyColumns: new string[] { "Faculty" },
                keyValues: new object[] { 4 },
                keyColumnTypes: new string[] { typeof(int).ToString() },
                columns: new string[] { "FacultyId" },
                columnTypes: new string[] { typeof(Guid).ToString() },
                values: new object[] {
                    new Guid("30cfd05a-662a-11ed-9022-0242ac120002")});

            migrationBuilder.UpdateData(
                table: "Courses",
                keyColumns: new string[] { "Faculty" },
                keyValues: new object[] { 8 },
                keyColumnTypes: new string[] { typeof(int).ToString() },
                columns: new string[] { "FacultyId" },
                columnTypes: new string[] { typeof(Guid).ToString() },
                values: new object[] {
                    new Guid("30cfd26c-662a-11ed-9022-0242ac120002")});

            migrationBuilder.UpdateData(
                table: "Courses",
                keyColumns: new string[] { "Faculty" },
                keyValues: new object[] { 16 },
                keyColumnTypes: new string[] { typeof(int).ToString() },
                columns: new string[] { "FacultyId" },
                columnTypes: new string[] { typeof(Guid).ToString() },
                values: new object[] {
                    new Guid("30cfd3de-662a-11ed-9022-0242ac120002")});

            migrationBuilder.UpdateData(
                table: "Courses",
                keyColumns: new string[] { "Faculty" },
                keyValues: new object[] { 32 },
                keyColumnTypes: new string[] { typeof(int).ToString() },
                columns: new string[] { "FacultyId" },
                columnTypes: new string[] { typeof(Guid).ToString() },
                values: new object[] {
                    new Guid("30cfda8c-662a-11ed-9022-0242ac120002")});

            migrationBuilder.UpdateData(
                table: "Courses",
                keyColumns: new string[] { "Faculty" },
                keyValues: new object[] { 64 },
                keyColumnTypes: new string[] { typeof(int).ToString() },
                columns: new string[] { "FacultyId" },
                columnTypes: new string[] { typeof(Guid).ToString() },
                values: new object[] {
                    new Guid("30cfd51e-662a-11ed-9022-0242ac120002") });

            migrationBuilder.DropForeignKey(
                name: "FK_Courses_Staff_HeadTeacherId",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "Faculty",
                table: "Staff");

            migrationBuilder.DropColumn(
                name: "Faculty",
                table: "MSTeamOperations");

            migrationBuilder.DropColumn(
                name: "Faculty",
                table: "Courses");

            migrationBuilder.RenameColumn(
                name: "HeadTeacherId",
                table: "Courses",
                newName: "StaffId");

            migrationBuilder.RenameIndex(
                name: "IX_Courses_HeadTeacherId",
                table: "Courses",
                newName: "IX_Courses_StaffId");

            migrationBuilder.AddColumn<Guid>(
                name: "FacultyId",
                table: "MSTeamOperations",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "MandatoryTraining_Modules",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "MandatoryTraining_CompletionRecords",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_MSTeamOperations_FacultyId",
                table: "MSTeamOperations",
                column: "FacultyId");

            migrationBuilder.CreateIndex(
                name: "IX_Courses_FacultyId",
                table: "Courses",
                column: "FacultyId");

            migrationBuilder.CreateIndex(
                name: "IX_FacultyMembership_FacultyId",
                table: "FacultyMembership",
                column: "FacultyId");

            migrationBuilder.CreateIndex(
                name: "IX_FacultyMembership_StaffId",
                table: "FacultyMembership",
                column: "StaffId");

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_Faculties_FacultyId",
                table: "Courses",
                column: "FacultyId",
                principalTable: "Faculties",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_Staff_StaffId",
                table: "Courses",
                column: "StaffId",
                principalTable: "Staff",
                principalColumn: "StaffId");

            migrationBuilder.AddForeignKey(
                name: "FK_MSTeamOperations_Faculties_FacultyId",
                table: "MSTeamOperations",
                column: "FacultyId",
                principalTable: "Faculties",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Courses_Faculties_FacultyId",
                table: "Courses");

            migrationBuilder.DropForeignKey(
                name: "FK_Courses_Staff_StaffId",
                table: "Courses");

            migrationBuilder.DropForeignKey(
                name: "FK_MSTeamOperations_Faculties_FacultyId",
                table: "MSTeamOperations");

            migrationBuilder.DropTable(
                name: "FacultyMembership");

            migrationBuilder.DropTable(
                name: "Faculties");

            migrationBuilder.DropIndex(
                name: "IX_MSTeamOperations_FacultyId",
                table: "MSTeamOperations");

            migrationBuilder.DropIndex(
                name: "IX_Courses_FacultyId",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "FacultyId",
                table: "MSTeamOperations");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "MandatoryTraining_Modules");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "MandatoryTraining_CompletionRecords");

            migrationBuilder.DropColumn(
                name: "FacultyId",
                table: "Courses");

            migrationBuilder.RenameColumn(
                name: "StaffId",
                table: "Courses",
                newName: "HeadTeacherId");

            migrationBuilder.RenameIndex(
                name: "IX_Courses_StaffId",
                table: "Courses",
                newName: "IX_Courses_HeadTeacherId");

            migrationBuilder.AddColumn<int>(
                name: "Faculty",
                table: "Staff",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Faculty",
                table: "MSTeamOperations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Faculty",
                table: "Courses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_Staff_HeadTeacherId",
                table: "Courses",
                column: "HeadTeacherId",
                principalTable: "Staff",
                principalColumn: "StaffId");
        }
    }
}
