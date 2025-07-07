using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    /// <inheritdoc />
    public partial class RemoveExtraStaffIdColumnInFacultyMemberships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Faculties_Memberships_Members_StaffMemberId",
                table: "Faculties_Memberships");

            migrationBuilder.DropIndex(
                name: "IX_Faculties_Memberships_StaffMemberId",
                table: "Faculties_Memberships");

            migrationBuilder.DropColumn(
                name: "StaffMemberId",
                table: "Faculties_Memberships");

            migrationBuilder.CreateIndex(
                name: "IX_Faculties_Memberships_StaffId",
                table: "Faculties_Memberships",
                column: "StaffId");

            migrationBuilder.AddForeignKey(
                name: "FK_Faculties_Memberships_Members_StaffId",
                table: "Faculties_Memberships",
                column: "StaffId",
                principalSchema: "Staff",
                principalTable: "Members",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Faculties_Memberships_Members_StaffId",
                table: "Faculties_Memberships");

            migrationBuilder.DropIndex(
                name: "IX_Faculties_Memberships_StaffId",
                table: "Faculties_Memberships");

            migrationBuilder.AddColumn<Guid>(
                name: "StaffMemberId",
                table: "Faculties_Memberships",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Faculties_Memberships_StaffMemberId",
                table: "Faculties_Memberships",
                column: "StaffMemberId");

            migrationBuilder.AddForeignKey(
                name: "FK_Faculties_Memberships_Members_StaffMemberId",
                table: "Faculties_Memberships",
                column: "StaffMemberId",
                principalSchema: "Staff",
                principalTable: "Members",
                principalColumn: "Id");
        }
    }
}
