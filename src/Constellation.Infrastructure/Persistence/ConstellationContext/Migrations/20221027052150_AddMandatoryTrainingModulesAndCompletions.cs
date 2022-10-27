using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    public partial class AddMandatoryTrainingModulesAndCompletions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MandatoryTraining_Modules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Expiry = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MandatoryTraining_Modules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MandatoryTraining_CompletionRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StaffId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CompletedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrainingModuleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModuleId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MandatoryTraining_CompletionRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MandatoryTraining_CompletionRecords_MandatoryTraining_Modules_ModuleId",
                        column: x => x.ModuleId,
                        principalTable: "MandatoryTraining_Modules",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MandatoryTraining_CompletionRecords_MandatoryTraining_Modules_TrainingModuleId",
                        column: x => x.TrainingModuleId,
                        principalTable: "MandatoryTraining_Modules",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MandatoryTraining_CompletionRecords_Staff_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Staff",
                        principalColumn: "StaffId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_MandatoryTraining_CompletionRecords_ModuleId",
                table: "MandatoryTraining_CompletionRecords",
                column: "ModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_MandatoryTraining_CompletionRecords_StaffId",
                table: "MandatoryTraining_CompletionRecords",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_MandatoryTraining_CompletionRecords_TrainingModuleId",
                table: "MandatoryTraining_CompletionRecords",
                column: "TrainingModuleId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MandatoryTraining_CompletionRecords");

            migrationBuilder.DropTable(
                name: "MandatoryTraining_Modules");
        }
    }
}
