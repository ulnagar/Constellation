using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    public partial class AddGroupTutorialModels : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmailQueue");

            migrationBuilder.CreateTable(
                name: "GroupTutorials_Tutorial",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
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
                    table.PrimaryKey("PK_GroupTutorials_Tutorial", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OutboxMessageConsumer",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxMessageConsumer", x => new { x.Id, x.Name });
                });

            migrationBuilder.CreateTable(
                name: "OutboxMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OccurredOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProcessedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Error = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GroupTutorials_Enrolment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    TutorialId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EffectiveTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GroupTutorialId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupTutorials_Enrolment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroupTutorials_Enrolment_GroupTutorials_Tutorial_GroupTutorialId",
                        column: x => x.GroupTutorialId,
                        principalTable: "GroupTutorials_Tutorial",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_GroupTutorials_Enrolment_GroupTutorials_Tutorial_TutorialId",
                        column: x => x.TutorialId,
                        principalTable: "GroupTutorials_Tutorial",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GroupTutorials_Enrolment_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "StudentId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GroupTutorials_Roll",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TutorialId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StaffId = table.Column<string>(type: "nvarchar(450)", nullable: true),
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
                    table.PrimaryKey("PK_GroupTutorials_Roll", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroupTutorials_Roll_GroupTutorials_Tutorial_TutorialId",
                        column: x => x.TutorialId,
                        principalTable: "GroupTutorials_Tutorial",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GroupTutorials_Roll_Staff_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Staff",
                        principalColumn: "StaffId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GroupTutorials_Teachers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StaffId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    TutorialId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EffectiveTo = table.Column<DateTime>(type: "datetime2", nullable: true),
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
                    table.PrimaryKey("PK_GroupTutorials_Teachers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroupTutorials_Teachers_GroupTutorials_Tutorial_TutorialId",
                        column: x => x.TutorialId,
                        principalTable: "GroupTutorials_Tutorial",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GroupTutorials_Teachers_Staff_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Staff",
                        principalColumn: "StaffId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GroupTutorials_RollStudent",
                columns: table => new
                {
                    StudentId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TutorialRollId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Present = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TutorialRollId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupTutorials_RollStudent", x => new { x.TutorialRollId, x.StudentId });
                    table.ForeignKey(
                        name: "FK_GroupTutorials_RollStudent_GroupTutorials_Roll_TutorialRollId",
                        column: x => x.TutorialRollId,
                        principalTable: "GroupTutorials_Roll",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GroupTutorials_RollStudent_GroupTutorials_Roll_TutorialRollId1",
                        column: x => x.TutorialRollId1,
                        principalTable: "GroupTutorials_Roll",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_GroupTutorials_RollStudent_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "StudentId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GroupTutorials_Enrolment_GroupTutorialId",
                table: "GroupTutorials_Enrolment",
                column: "GroupTutorialId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupTutorials_Enrolment_StudentId",
                table: "GroupTutorials_Enrolment",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupTutorials_Enrolment_TutorialId",
                table: "GroupTutorials_Enrolment",
                column: "TutorialId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupTutorials_Roll_StaffId",
                table: "GroupTutorials_Roll",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupTutorials_Roll_TutorialId",
                table: "GroupTutorials_Roll",
                column: "TutorialId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupTutorials_RollStudent_StudentId",
                table: "GroupTutorials_RollStudent",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupTutorials_RollStudent_TutorialRollId1",
                table: "GroupTutorials_RollStudent",
                column: "TutorialRollId1");

            migrationBuilder.CreateIndex(
                name: "IX_GroupTutorials_Teachers_StaffId",
                table: "GroupTutorials_Teachers",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupTutorials_Teachers_TutorialId",
                table: "GroupTutorials_Teachers",
                column: "TutorialId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GroupTutorials_Enrolment");

            migrationBuilder.DropTable(
                name: "GroupTutorials_RollStudent");

            migrationBuilder.DropTable(
                name: "GroupTutorials_Teachers");

            migrationBuilder.DropTable(
                name: "OutboxMessageConsumer");

            migrationBuilder.DropTable(
                name: "OutboxMessages");

            migrationBuilder.DropTable(
                name: "GroupTutorials_Roll");

            migrationBuilder.DropTable(
                name: "GroupTutorials_Tutorial");

            migrationBuilder.CreateTable(
                name: "EmailQueue",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReferenceId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReferenceType = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailQueue", x => x.Id);
                });
        }
    }
}
