using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTablesToRepairGrade : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Nominations
            migrationBuilder.Sql("ALTER TABLE [AwardNominations].[Nominations] ALTER COLUMN [Grade] nvarchar(3) NULL;");
            migrationBuilder.Sql(@"
        UPDATE [AwardNominations].[Nominations] SET [Grade] = NULL
        WHERE [AwardType] NOT IN ('First in Course',
                                  'First in Course - Mathematics',
                                  'First in Course - Science & Technology');");

            // WorkFlows_CaseDetails
            migrationBuilder.Sql("ALTER TABLE [dbo].[WorkFlows_CaseDetails] ALTER COLUMN [Grade] nvarchar(3) NULL;");
            migrationBuilder.Sql(@"
        UPDATE [dbo].[WorkFlows_CaseDetails] SET [Grade] = NULL
        WHERE [CaseDetailType] NOT IN ('AttendanceCaseDetail', 'ComplianceCaseDetail');");

            // ConsentRequirements
            migrationBuilder.Sql("ALTER TABLE [ThirdParty].[ConsentRequirements] ALTER COLUMN [Grade] nvarchar(3) NULL;");
            migrationBuilder.Sql(@"
        UPDATE [ThirdParty].[ConsentRequirements] SET [Grade] = NULL
        WHERE [Type] <> 'Grade';");
        }

        protected override void Down(MigrationBuilder migrationBuilder) { }
    }
}
