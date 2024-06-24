using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMandatoryTraining : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Training_Roles_Members");

            migrationBuilder.DropTable(
                name: "Training_Roles_Modules");

            migrationBuilder.DropTable(
                name: "Training_Roles_Roles");
            
            migrationBuilder.EnsureSchema(
                name: "Training");

            migrationBuilder.DropForeignKey(
                name: "FK_Training_Modules_Completions_Training_Modules_Modules_TrainingModuleId",
                table: "Training_Modules_Completions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Training_Modules_Moduels",
                table: "Training_Modules_Modules");

            migrationBuilder.RenameTable(
                name: "Training_Modules_Modules",
                newName: "Modules",
                newSchema: "Training");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Modules",
                table: "Modules",
                schema: "Training",
                column: "Id");

            migrationBuilder.DropForeignKey(
                name: "FK_Training_Modules_Completions_Staff_StaffId",
                table: "Training_Modules_Completions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Training_Modules_Completions",
                table: "Training_Modules_Completions");

            migrationBuilder.RenameTable(
                name: "Training_Modules_Completions",
                newName: "Completions",
                newSchema: "Training");

            migrationBuilder.AlterColumn<Guid>(
                name: "TrainingModuleId",
                table: "Completions",
                schema: "Training",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Completions",
                table: "Completions",
                schema: "Training",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Completions_Modules_TrainingModuleId",
                table: "Completions",
                schema: "Training",
                column: "TrainingModuleId",
                principalSchema: "Training",
                principalTable: "Modules",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Completions_Staff_StaffId",
                table: "Completions",
                schema: "Training",
                column: "StaffId",
                principalTable: "Staff",
                principalColumn: "StaffId");

            migrationBuilder.CreateTable(
                name: "Assignees",
                schema: "Training",
                columns: table => new
                {
                    ModuleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StaffId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assignees", x => new { x.ModuleId, x.StaffId });
                    table.ForeignKey(
                        name: "FK_Assignees_Modules_ModuleId",
                        column: x => x.ModuleId,
                        principalSchema: "Training",
                        principalTable: "Modules",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Assignees_Staff_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Staff",
                        principalColumn: "StaffId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Assignees_StaffId",
                schema: "Training",
                table: "Assignees",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_Completions_StaffId",
                schema: "Training",
                table: "Completions",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_Completions_TrainingModuleId",
                schema: "Training",
                table: "Completions",
                column: "TrainingModuleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Assignees",
                schema: "Training");

            migrationBuilder.DropTable(
                name: "Completions",
                schema: "Training");

            migrationBuilder.DropTable(
                name: "Modules",
                schema: "Training");

            migrationBuilder.CreateTable(
                name: "Training_Modules_Modules",
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
                    table.PrimaryKey("PK_Training_Modules_Modules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Training_Roles_Roles",
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
                    table.PrimaryKey("PK_Training_Roles_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Training_Modules_Completions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrainingModuleId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CompletedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StaffId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Training_Modules_Completions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Training_Modules_Completions_Staff_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Staff",
                        principalColumn: "StaffId");
                    table.ForeignKey(
                        name: "FK_Training_Modules_Completions_Training_Modules_Modules_TrainingModuleId",
                        column: x => x.TrainingModuleId,
                        principalTable: "Training_Modules_Modules",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Training_Roles_Members",
                columns: table => new
                {
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StaffId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Training_Roles_Members", x => new { x.RoleId, x.StaffId });
                    table.ForeignKey(
                        name: "FK_Training_Roles_Members_Staff_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Staff",
                        principalColumn: "StaffId");
                    table.ForeignKey(
                        name: "FK_Training_Roles_Members_Training_Roles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Training_Roles_Roles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Training_Roles_Modules",
                columns: table => new
                {
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModuleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Training_Roles_Modules", x => new { x.RoleId, x.ModuleId });
                    table.ForeignKey(
                        name: "FK_Training_Roles_Modules_Training_Modules_Modules_ModuleId",
                        column: x => x.ModuleId,
                        principalTable: "Training_Modules_Modules",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Training_Roles_Modules_Training_Roles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Training_Roles_Roles",
                        principalColumn: "Id");
                });

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
    }
}
