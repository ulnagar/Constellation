using System;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSubjectAggregate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Start Custom Code

            migrationBuilder.DropTable(
                name: "OfferingResources");

            migrationBuilder.DropIndex(
                name: "IX_Courses_FacultyId",
                table: "Courses");

            migrationBuilder.DropIndex(
                name: "IX_Offerings_CourseId",
                table: "Offerings");

            migrationBuilder.DropIndex(
                name: "IX_Sessions_PeriodId",
                table: "Sessions");

            migrationBuilder.DropIndex(
                name: "IX_Sessions_RoomId",
                table: "Sessions");

            migrationBuilder.DropIndex(
                name: "IX_Sessions_StaffId",
                table: "Sessions");

            migrationBuilder.DropForeignKey(
                name: "FK_Sessions_Offerings_OfferingId",
                table: "Sessions");

            migrationBuilder.DropForeignKey(
                name: "FK_Sessions_Periods_PeriodId",
                table: "Sessions");

            migrationBuilder.DropForeignKey(
                name: "FK_Sessions_Rooms_RoomId",
                table: "Sessions");

            migrationBuilder.DropForeignKey(
                name: "FK_Sessions_Staff_StaffId",
                table: "Sessions");

            migrationBuilder.DropForeignKey(
                name: "FK_Courses_Faculties_FacultyId",
                table: "Courses");

            migrationBuilder.DropForeignKey(
                name: "FK_Absences_Absences_Offerings_OfferingId",
                table: "Absences_Absences");

            migrationBuilder.DropForeignKey(
                name: "FK_Assignments_Assignments_Courses_CourseId",
                table: "Assignments_Assignments");

            migrationBuilder.DropForeignKey(
                name: "FK_Covers_ClassCovers_Offerings_OfferingId",
                table: "Covers_ClassCovers");

            migrationBuilder.DropForeignKey(
                name: "FK_Enrolments_Offerings_OfferingId",
                table: "Enrolments");

            migrationBuilder.DropForeignKey(
                name: "FK_MSTeamOperations_Offerings_OfferingId",
                table: "MSTeamOperations");

            migrationBuilder.DropForeignKey(
                name: "FK_PartialAbsences_Offerings_OfferingId",
                table: "PartialAbsences");

            migrationBuilder.DropForeignKey(
                name: "FK_SciencePracs_Lessons_Offerings_Offerings_OfferingId",
                table: "SciencePracs_Lessons_Offerings");

            migrationBuilder.DropForeignKey(
                name: "FK_WholeAbsences_Offerings_OfferingId",
                table: "WholeAbsences");

            migrationBuilder.DropForeignKey(
                name: "FK_Offerings_Courses_CourseId",
                table: "Offerings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Courses",
                table: "Courses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Offerings",
                table: "Offerings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Sessions",
                table: "Sessions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SciencePracs_Lessons_Offerings",
                table: "SciencePracs_Lessons_Offerings");

            migrationBuilder.RenameTable(
                name: "Courses",
                newName: "Subjects_Courses");

            migrationBuilder.RenameTable(
                name: "Offerings",
                newName: "Subjects_Offerings");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Subjects_Offerings",
                newName: "OldId");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "Subjects_Offerings",
                type: "uniqueidentifier",
                nullable: true);

            // Generate new IDs for Offerings
            migrationBuilder.Sql(
                @"UPDATE [dbo].[Subjects_Offerings]
                    SET [Id] = NEWID()
                    WHERE 1 = 1;");

            migrationBuilder.RenameColumn(
                name: "OfferingId",
                table: "Absences_Absences",
                newName: "xOfferingId");

            migrationBuilder.AddColumn<Guid>(
                name: "OfferingId",
                table: "Absences_Absences",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql(
                @"UPDATE [dbo].[Absences_Absences]
                    SET [OfferingId] = [Subjects_Offerings].[Id]
                    FROM [dbo].[Absences_Absences]
                    INNER JOIN [dbo].[Subjects_Offerings]
                    ON [dbo].[Absences_Absences].[xOfferingId] = [Subjects_Offerings].[OldId];");

            migrationBuilder.DropIndex(
                name: "IX_Absences_Absences_OfferingId",
                table: "Absences_Absences");

            migrationBuilder.DropColumn(
                name: "xOfferingId",
                table: "Absences_Absences");

            migrationBuilder.RenameColumn(
                name: "OfferingId",
                table: "Awards_Nominations",
                newName: "xOfferingId");

            migrationBuilder.AddColumn<Guid>(
                name: "OfferingId",
                table: "Awards_Nominations",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql(
                @"UPDATE [dbo].[Awards_Nominations]
                    SET [OfferingId] = [Subjects_Offerings].[Id]
                    FROM [dbo].[Awards_Nominations]
                    INNER JOIN [dbo].[Subjects_Offerings]
                    ON [dbo].[Awards_Nominations].[xOfferingId] = [Subjects_Offerings].[OldId];");

            migrationBuilder.DropColumn(
                name: "xOfferingId",
                table: "Awards_Nominations");

            migrationBuilder.RenameColumn(
                name: "OfferingId",
                table: "Covers_ClassCovers",
                newName: "xOfferingId");

            migrationBuilder.AddColumn<Guid>(
                name: "OfferingId",
                table: "Covers_ClassCovers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql(
                @"UPDATE [dbo].[Covers_ClassCovers]
                    SET [OfferingId] = [Subjects_Offerings].[Id]
                    FROM [dbo].[Covers_ClassCovers]
                    INNER JOIN [dbo].[Subjects_Offerings]
                    ON [dbo].[Covers_ClassCovers].[xOfferingId] = [Subjects_Offerings].[OldId];");

            migrationBuilder.DropIndex(
                name: "IX_Covers_ClassCovers_OfferingId",
                table: "Covers_ClassCovers");

            migrationBuilder.DropColumn(
                name: "xOfferingId",
                table: "Covers_ClassCovers");

            migrationBuilder.RenameColumn(
                name: "OfferingId",
                table: "Enrolments",
                newName: "xOfferingId");

            migrationBuilder.AddColumn<Guid>(
                name: "OfferingId",
                table: "Enrolments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql(
                @"UPDATE [dbo].[Enrolments]
                    SET [OfferingId] = [Subjects_Offerings].[Id]
                    FROM [dbo].[Enrolments]
                    INNER JOIN [dbo].[Subjects_Offerings]
                    ON [dbo].[Enrolments].[xOfferingId] = [Subjects_Offerings].[OldId];");

            migrationBuilder.DropIndex(
                name: "IX_Enrolments_OfferingId",
                table: "Enrolments");

            migrationBuilder.DropColumn(
                name: "xOfferingId",
                table: "Enrolments");

            migrationBuilder.RenameColumn(
                name: "OfferingId",
                table: "MSTeamOperations",
                newName: "xOfferingId");

            migrationBuilder.AddColumn<Guid>(
                name: "OfferingId",
                table: "MSTeamOperations",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql(
                @"UPDATE [dbo].[MSTeamOperations]
                    SET [OfferingId] = [Subjects_Offerings].[Id]
                    FROM [dbo].[MSTeamOperations]
                    INNER JOIN [dbo].[Subjects_Offerings]
                    ON [dbo].[MSTeamOperations].[xOfferingId] = [Subjects_Offerings].[OldId];");

            migrationBuilder.DropIndex(
                name: "IX_MSTeamOperations_OfferingId",
                table: "MSTeamOperations");

            migrationBuilder.DropColumn(
                name: "xOfferingId",
                table: "MSTeamOperations");

            migrationBuilder.RenameColumn(
                name: "OfferingId",
                table: "PartialAbsences",
                newName: "xOfferingId");

            migrationBuilder.AddColumn<Guid>(
                name: "OfferingId",
                table: "PartialAbsences",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql(
                @"UPDATE [dbo].[PartialAbsences]
                    SET [OfferingId] = [Subjects_Offerings].[Id]
                    FROM [dbo].[PartialAbsences]
                    INNER JOIN [dbo].[Subjects_Offerings]
                    ON [dbo].[PartialAbsences].[xOfferingId] = [Subjects_Offerings].[OldId];");

            migrationBuilder.DropIndex(
                name: "IX_PartialAbsences_OfferingId",
                table: "PartialAbsences");

            migrationBuilder.DropColumn(
                name: "xOfferingId",
                table: "PartialAbsences");

            migrationBuilder.RenameColumn(
                name: "OfferingId",
                table: "SciencePracs_Lessons_Offerings",
                newName: "xOfferingId");

            migrationBuilder.AddColumn<Guid>(
                name: "OfferingId",
                table: "SciencePracs_Lessons_Offerings",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql(
                @"UPDATE [dbo].[SciencePracs_Lessons_Offerings]
                    SET [OfferingId] = [Subjects_Offerings].[Id]
                    FROM [dbo].[SciencePracs_Lessons_Offerings]
                    INNER JOIN [dbo].[Subjects_Offerings]
                    ON [dbo].[SciencePracs_Lessons_Offerings].[xOfferingId] = [Subjects_Offerings].[OldId];");

            migrationBuilder.DropIndex(
                name: "IX_SciencePracs_Lessons_Offerings_OfferingId",
                table: "SciencePracs_Lessons_Offerings");

            migrationBuilder.DropColumn(
                name: "xOfferingId",
                table: "SciencePracs_Lessons_Offerings");

            migrationBuilder.AlterColumn<Guid>(
                name: "OfferingId",
                table: "SciencePracs_Lessons_Offerings",
                nullable: false);

            migrationBuilder.RenameColumn(
                name: "OfferingId",
                table: "WholeAbsences",
                newName: "xOfferingId");

            migrationBuilder.AddColumn<Guid>(
                name: "OfferingId",
                table: "WholeAbsences",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql(
                @"UPDATE [dbo].[WholeAbsences]
                    SET [OfferingId] = [Subjects_Offerings].[Id]
                    FROM [dbo].[WholeAbsences]
                    INNER JOIN [dbo].[Subjects_Offerings]
                    ON [dbo].[WholeAbsences].[xOfferingId] = [Subjects_Offerings].[OldId];");

            migrationBuilder.DropIndex(
                name: "IX_WholeAbsences_OfferingId",
                table: "WholeAbsences");

            migrationBuilder.DropColumn(
                name: "xOfferingId",
                table: "WholeAbsences");

            migrationBuilder.RenameColumn(
                name: "OfferingId",
                table: "Sessions",
                newName: "xOfferingId");

            migrationBuilder.AddColumn<Guid>(
                name: "OfferingId",
                table: "Sessions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql(
                @"UPDATE [dbo].[Sessions]
                    SET [OfferingId] = [Subjects_Offerings].[Id]
                    FROM [dbo].[Sessions]
                    INNER JOIN [dbo].[Subjects_Offerings]
                    ON [dbo].[Sessions].[xOfferingId] = [Subjects_Offerings].[OldId];");

            migrationBuilder.DropIndex(
                name: "IX_Sessions_OfferingId",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "xOfferingId",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "OldId",
                table: "Subjects_Offerings");

            migrationBuilder.RenameTable(
                name: "Sessions",
                newName: "Subjects_Sessions");

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Subjects_Courses",
                type: "nvarchar(3)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Subjects_Resource",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OfferingId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ResourceId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OfferingId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subjects_Resource", x => x.Id);
                });

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Subjects_Offerings",
                nullable: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Subjects_Offerings",
                table: "Subjects_Offerings",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SciencePracs_Lessons_Offerings",
                table: "SciencePracs_Lessons_Offerings",
                columns: new string[] { "LessonId", "OfferingId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Subjects_Sessions",
                table: "Subjects_Sessions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Subjects_Courses",
                table: "Subjects_Courses",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Subjects_Resource_Subjects_Offerings_OfferingId",
                table: "Subjects_Resource",
                column: "OfferingId",
                principalTable: "Subjects_Offerings",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Subjects_Courses_Faculties_FacultyId",
                table: "Subjects_Courses",
                column: "FacultyId",
                principalTable: "Faculties",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Subjects_Offerings_Subjects_Courses_CourseId",
                table: "Subjects_Offerings",
                column: "CourseId",
                principalTable: "Subjects_Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Subjects_Sessions_Periods_PeriodId",
                table: "Subjects_Sessions",
                column: "PeriodId",
                principalTable: "Periods",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Subjects_Sessions_Rooms_RoomId",
                table: "Subjects_Sessions",
                column: "RoomId",
                principalTable: "Rooms",
                principalColumn: "ScoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Subjects_Sessions_Staff_StaffId",
                table: "Subjects_Sessions",
                column: "StaffId",
                principalTable: "Staff",
                principalColumn: "StaffId");

            migrationBuilder.AddForeignKey(
                name: "FK_Subjects_Sessions_Subjects_Offerings_OfferingId",
                table: "Subjects_Sessions",
                column: "OfferingId",
                principalTable: "Subjects_Offerings",
                principalColumn: "Id");

            // End Custom Code

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_Courses_FacultyId",
                table: "Subjects_Courses",
                column: "FacultyId");

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_Offerings_CourseId",
                table: "Subjects_Offerings",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_Resource_OfferingId",
                table: "Subjects_Resource",
                column: "OfferingId");

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_Resource_OfferingId1",
                table: "Subjects_Resource",
                column: "OfferingId1");

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_Sessions_OfferingId",
                table: "Subjects_Sessions",
                column: "OfferingId");

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_Sessions_PeriodId",
                table: "Subjects_Sessions",
                column: "PeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_Sessions_RoomId",
                table: "Subjects_Sessions",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_Sessions_StaffId",
                table: "Subjects_Sessions",
                column: "StaffId");

            migrationBuilder.AddForeignKey(
                name: "FK_Absences_Absences_Subjects_Offerings_OfferingId",
                table: "Absences_Absences",
                column: "OfferingId",
                principalTable: "Subjects_Offerings",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Assignments_Assignments_Subjects_Courses_CourseId",
                table: "Assignments_Assignments",
                column: "CourseId",
                principalTable: "Subjects_Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Covers_ClassCovers_Subjects_Offerings_OfferingId",
                table: "Covers_ClassCovers",
                column: "OfferingId",
                principalTable: "Subjects_Offerings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Enrolments_Subjects_Offerings_OfferingId",
                table: "Enrolments",
                column: "OfferingId",
                principalTable: "Subjects_Offerings",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MSTeamOperations_Subjects_Offerings_OfferingId",
                table: "MSTeamOperations",
                column: "OfferingId",
                principalTable: "Subjects_Offerings",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PartialAbsences_Subjects_Offerings_OfferingId",
                table: "PartialAbsences",
                column: "OfferingId",
                principalTable: "Subjects_Offerings",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SciencePracs_Lessons_Offerings_Subjects_Offerings_OfferingId",
                table: "SciencePracs_Lessons_Offerings",
                column: "OfferingId",
                principalTable: "Subjects_Offerings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WholeAbsences_Subjects_Offerings_OfferingId",
                table: "WholeAbsences",
                column: "OfferingId",
                principalTable: "Subjects_Offerings",
                principalColumn: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_WholeAbsences_OfferingId",
                table: "WholeAbsences",
                column: "OfferingId");

            migrationBuilder.CreateIndex(
                name: "IX_MSTeamOperations_OfferingId",
                table: "MSTeamOperations",
                column: "OfferingId");

            migrationBuilder.CreateIndex(
                name: "IX_Absences_Absences_OfferingId",
                table: "Absences_Absences",
                column: "OfferingId");

            migrationBuilder.CreateIndex(
                name: "IX_Covers_ClassCovers_OfferignId",
                table: "Covers_ClassCovers",
                column: "OfferingId");

            migrationBuilder.CreateIndex(
                name: "IX_SciencePracs_Lessons_Offerings_OfferingId",
                table: "SciencePracs_Lessons_Offerings",
                column: "OfferingId");

            migrationBuilder.CreateIndex(
                name: "IX_Enrolments_OfferingId",
                table: "Enrolments",
                column: "OfferingId");

            migrationBuilder.CreateIndex(
                name: "IX_PartialAbsences_OfferingId",
                table: "PartialAbsences",
                column: "OfferingId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Absences_Absences_Subjects_Offerings_OfferingId",
                table: "Absences_Absences");

            migrationBuilder.DropForeignKey(
                name: "FK_Assignments_Assignments_Subjects_Courses_CourseId",
                table: "Assignments_Assignments");

            migrationBuilder.DropForeignKey(
                name: "FK_Covers_ClassCovers_Subjects_Offerings_OfferingId",
                table: "Covers_ClassCovers");

            migrationBuilder.DropForeignKey(
                name: "FK_Enrolments_Subjects_Offerings_OfferingId",
                table: "Enrolments");

            migrationBuilder.DropForeignKey(
                name: "FK_MSTeamOperations_Subjects_Offerings_OfferingId",
                table: "MSTeamOperations");

            migrationBuilder.DropForeignKey(
                name: "FK_PartialAbsences_Subjects_Offerings_OfferingId",
                table: "PartialAbsences");

            migrationBuilder.DropForeignKey(
                name: "FK_SciencePracs_Lessons_Offerings_Subjects_Offerings_OfferingId",
                table: "SciencePracs_Lessons_Offerings");

            migrationBuilder.DropForeignKey(
                name: "FK_WholeAbsences_Subjects_Offerings_OfferingId",
                table: "WholeAbsences");

            migrationBuilder.DropTable(
                name: "Subjects_Resource");

            migrationBuilder.DropTable(
                name: "Subjects_Sessions");

            migrationBuilder.DropTable(
                name: "Subjects_Offerings");

            migrationBuilder.DropTable(
                name: "Subjects_Courses");

            migrationBuilder.AlterColumn<int>(
                name: "OfferingId",
                table: "WholeAbsences",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "OfferingId",
                table: "SciencePracs_Lessons_Offerings",
                type: "int",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<int>(
                name: "OfferingId",
                table: "PartialAbsences",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "OfferingId",
                table: "MSTeamOperations",
                type: "int",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "OfferingId",
                table: "Enrolments",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "OfferingId",
                table: "Covers_ClassCovers",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "OfferingId",
                table: "Awards_Nominations",
                type: "int",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "OfferingId",
                table: "Absences_Absences",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "Courses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FacultyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FullTimeEquivalentValue = table.Column<decimal>(type: "decimal(4,3)", precision: 4, scale: 3, nullable: false),
                    Grade = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Courses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Courses_Faculties_FacultyId",
                        column: x => x.FacultyId,
                        principalTable: "Faculties",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Offerings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CourseId = table.Column<int>(type: "int", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Offerings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Offerings_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OfferingResources",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OfferingId = table.Column<int>(type: "int", nullable: false),
                    ShowLink = table.Column<bool>(type: "bit", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UrlPath = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OfferingResources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OfferingResources_Offerings_OfferingId",
                        column: x => x.OfferingId,
                        principalTable: "Offerings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Sessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OfferingId = table.Column<int>(type: "int", nullable: false),
                    PeriodId = table.Column<int>(type: "int", nullable: false),
                    RoomId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    StaffId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateDeleted = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sessions_Offerings_OfferingId",
                        column: x => x.OfferingId,
                        principalTable: "Offerings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Sessions_Periods_PeriodId",
                        column: x => x.PeriodId,
                        principalTable: "Periods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Sessions_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Rooms",
                        principalColumn: "ScoId");
                    table.ForeignKey(
                        name: "FK_Sessions_Staff_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Staff",
                        principalColumn: "StaffId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Courses_FacultyId",
                table: "Courses",
                column: "FacultyId");

            migrationBuilder.CreateIndex(
                name: "IX_OfferingResources_OfferingId",
                table: "OfferingResources",
                column: "OfferingId");

            migrationBuilder.CreateIndex(
                name: "IX_Offerings_CourseId",
                table: "Offerings",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_OfferingId",
                table: "Sessions",
                column: "OfferingId");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_PeriodId",
                table: "Sessions",
                column: "PeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_RoomId",
                table: "Sessions",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_StaffId",
                table: "Sessions",
                column: "StaffId");

            migrationBuilder.AddForeignKey(
                name: "FK_Absences_Absences_Offerings_OfferingId",
                table: "Absences_Absences",
                column: "OfferingId",
                principalTable: "Offerings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Assignments_Assignments_Courses_CourseId",
                table: "Assignments_Assignments",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Covers_ClassCovers_Offerings_OfferingId",
                table: "Covers_ClassCovers",
                column: "OfferingId",
                principalTable: "Offerings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Enrolments_Offerings_OfferingId",
                table: "Enrolments",
                column: "OfferingId",
                principalTable: "Offerings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MSTeamOperations_Offerings_OfferingId",
                table: "MSTeamOperations",
                column: "OfferingId",
                principalTable: "Offerings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PartialAbsences_Offerings_OfferingId",
                table: "PartialAbsences",
                column: "OfferingId",
                principalTable: "Offerings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SciencePracs_Lessons_Offerings_Offerings_OfferingId",
                table: "SciencePracs_Lessons_Offerings",
                column: "OfferingId",
                principalTable: "Offerings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WholeAbsences_Offerings_OfferingId",
                table: "WholeAbsences",
                column: "OfferingId",
                principalTable: "Offerings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
