using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    using Microsoft.EntityFrameworkCore.Metadata.Internal;

    /// <inheritdoc />
    public partial class SplitTrainingAggregateIntoRoleAndModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MandatoryTraining_RoleModules_ModuleId",
                table: "MandatoryTraining_RoleModules");

            migrationBuilder.DropIndex(
                name: "IX_MandatoryTraining_RoleMembers_StaffId",
                table: "MandatoryTraining_RoleMembers");

            migrationBuilder.DropIndex(
                name: "IX_MandatoryTraining_Completions_TrainingModuleId",
                table: "MandatoryTraining_Completions");

            migrationBuilder.DropIndex(
                name: "IX_MandatoryTraining_Completions_StaffId",
                table: "MandatoryTraining_Completions");

            migrationBuilder.DropForeignKey(
                name: "FK_MandatoryTraining_RoleModules_MandatoryTraining_Roles_RoleId",
                table: "MandatoryTraining_RoleModules");

            migrationBuilder.DropForeignKey(
                name: "FK_MandatoryTraining_RoleModules_MandatoryTraining_Modules_ModuleId",
                table: "MandatoryTraining_RoleModules");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MandatoryTraining_RoleModules",
                table: "MandatoryTraining_RoleModules");

            migrationBuilder.DropForeignKey(
                name: "FK_MandatoryTraining_RoleMembers_Staff_StaffId",
                table: "MandatoryTraining_RoleMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_MandatoryTraining_RoleMembers_MandatoryTraining_Roles_RoleId",
                table: "MandatoryTraining_RoleMembers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MandatoryTraining_RoleMembers",
                table: "MandatoryTraining_RoleMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_MandatoryTraining_Completions_Staff_StaffId",
                table: "MandatoryTraining_Completions");

            migrationBuilder.DropForeignKey(
                name: "FK_MandatoryTraining_Completions_MandatoryTraining_Modules_TrainingModuleId",
                table: "MandatoryTraining_Completions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MandatoryTraining_Completions",
                table: "MandatoryTraining_Completions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MandatoryTraining_Roles",
                table: "MandatoryTraining_Roles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MandatoryTraining_Modules",
                table: "MandatoryTraining_Modules");

            migrationBuilder.RenameTable(
                name: "MandatoryTraining_Modules",
                newName: "Training_Modules_Modules");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Training_Modules_Moduels",
                table: "Training_Modules_Modules",
                column: "Id");

            migrationBuilder.RenameTable(
                name: "MandatoryTraining_Roles",
                newName: "Training_Roles_Roles");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Training_Roles_Roles",
                table: "Training_Roles_Roles",
                column: "Id");

            migrationBuilder.Sql(
                @"DELETE FROM [dbo].[MandatoryTraining_Completions]
                    WHERE [CompletedDate] is null");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CompletedDate",
                table: "MandatoryTraining_Completions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1900, 1, 1));

            migrationBuilder.DropColumn(
                name: "NotRequired",
                table: "MandatoryTraining_Completions");

            migrationBuilder.RenameTable(
                name: "MandatoryTraining_Completions",
                newName: "Training_Modules_Completions");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Training_Modules_Completions",
                table: "Training_Modules_Completions",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Training_Modules_Completions_Staff_StaffId",
                table: "Training_Modules_Completions",
                column: "StaffId",
                principalTable: "Staff",
                principalColumn: "StaffId");

            migrationBuilder.AddForeignKey(
                name: "FK_Training_Modules_Completions_Training_Modules_Modules_TrainingModuleId",
                table: "Training_Modules_Completions",
                column: "TrainingModuleId",
                principalTable: "Training_Modules_Modules",
                principalColumn: "Id");

            migrationBuilder.RenameTable(
                name: "MandatoryTraining_RoleMembers",
                newName: "Training_Roles_Members");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Training_Roles_Members",
                table: "Training_Roles_Members",
                columns: new[] { "RoleId", "StaffId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Training_Roles_Members_Staff_StaffId",
                table: "Training_Roles_Members",
                column: "StaffId",
                principalTable: "Staff",
                principalColumn: "StaffId");

            migrationBuilder.AddForeignKey(
                name: "FK_Training_Roles_Members_Training_Roles_Roles_RoleId",
                table: "Training_Roles_Members",
                column: "RoleId",
                principalTable: "Training_Roles_Roles",
                principalColumn: "Id");

            migrationBuilder.RenameTable(
                name: "MandatoryTraining_RoleModules",
                newName: "Training_Roles_Modules");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Training_Roles_Modules",
                table: "Training_Roles_Modules",
                columns: new[] { "RoleId", "ModuleId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Training_Roles_Modules_Training_Modules_Modules_ModuleId",
                table: "Training_Roles_Modules",
                column: "ModuleId",
                principalTable: "Training_Modules_Modules",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Training_Roles_Modules_Training_Roles_Roles_RoleId",
                table: "Training_Roles_Modules",
                column: "RoleId",
                principalTable: "Training_Roles_Roles",
                principalColumn: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Training_Modules_Completions_StaffId",
                table: "Training_Modules_Completions",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_Training_Modules_Completions_TrainingModuleId",
                table: "Training_Modules_Completions",
                column: "TrainingModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_Training_Roles_Members_StaffId",
                table: "Training_Roles_Members",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_Training_Roles_Modules_ModuleId",
                table: "Training_Roles_Modules",
                column: "ModuleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Training_Modules_Completions");

            migrationBuilder.DropTable(
                name: "Training_Roles_Members");

            migrationBuilder.DropTable(
                name: "Training_Roles_Modules");

            migrationBuilder.DropTable(
                name: "Training_Modules_Modules");

            migrationBuilder.DropTable(
                name: "Training_Roles_Roles");

            migrationBuilder.CreateTable(
                name: "MandatoryTraining_Modules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Expiry = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MandatoryTraining_Modules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MandatoryTraining_Roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                    table.PrimaryKey("PK_MandatoryTraining_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MandatoryTraining_Completions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrainingModuleId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CompletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NotRequired = table.Column<bool>(type: "bit", nullable: false),
                    StaffId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MandatoryTraining_Completions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MandatoryTraining_Completions_MandatoryTraining_Modules_TrainingModuleId",
                        column: x => x.TrainingModuleId,
                        principalTable: "MandatoryTraining_Modules",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MandatoryTraining_Completions_Staff_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Staff",
                        principalColumn: "StaffId",
                        onDelete: ReferentialAction.Restrict);
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
                name: "IX_MandatoryTraining_Completions_StaffId",
                table: "MandatoryTraining_Completions",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_MandatoryTraining_Completions_TrainingModuleId",
                table: "MandatoryTraining_Completions",
                column: "TrainingModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_MandatoryTraining_RoleMembers_StaffId",
                table: "MandatoryTraining_RoleMembers",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_MandatoryTraining_RoleModules_ModuleId",
                table: "MandatoryTraining_RoleModules",
                column: "ModuleId");
        }
    }
}
