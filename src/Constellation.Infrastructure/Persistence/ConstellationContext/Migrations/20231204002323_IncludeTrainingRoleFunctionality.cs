using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    /// <inheritdoc />
    public partial class IncludeTrainingRoleFunctionality : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CanMarkNotRequired",
                table: "MandatoryTraining_Modules");

            migrationBuilder.CreateTable(
                name: "MandatoryTraining_Roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_MandatoryTraining_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MandatoryTraining_RoleMembers",
                columns: table => new
                {
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StaffId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MandatoryTraining_RoleMembers", x => new { x.RoleId, x.StaffId });
                    table.ForeignKey(
                        name: "FK_MandatoryTraining_RoleMembers_MandatoryTraining_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "MandatoryTraining_Roles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MandatoryTraining_RoleMembers_Staff_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Staff",
                        principalColumn: "StaffId");
                });

            migrationBuilder.CreateTable(
                name: "MandatoryTraining_RoleModules",
                columns: table => new
                {
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModuleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MandatoryTraining_RoleModules", x => new { x.RoleId, x.ModuleId });
                    table.ForeignKey(
                        name: "FK_MandatoryTraining_RoleModules_MandatoryTraining_Modules_ModuleId",
                        column: x => x.ModuleId,
                        principalTable: "MandatoryTraining_Modules",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MandatoryTraining_RoleModules_MandatoryTraining_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "MandatoryTraining_Roles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_MandatoryTraining_RoleMembers_StaffId",
                table: "MandatoryTraining_RoleMembers",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_MandatoryTraining_RoleModules_ModuleId",
                table: "MandatoryTraining_RoleModules",
                column: "ModuleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MandatoryTraining_RoleMembers");

            migrationBuilder.DropTable(
                name: "MandatoryTraining_RoleModules");

            migrationBuilder.DropTable(
                name: "MandatoryTraining_Roles");

            migrationBuilder.AddColumn<bool>(
                name: "CanMarkNotRequired",
                table: "MandatoryTraining_Modules",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
