using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Migrations
{
    /// <inheritdoc />
    public partial class ConvertGradeFromEnumToObject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            ConvertIntGradeColumnToString(migrationBuilder, "Assessments", "Students", "StudentGrade");
            ConvertIntGradeColumnToString(migrationBuilder, "Attendance", "CheckInResponses", "Grade");
            ConvertIntGradeColumnToString(migrationBuilder, "Attendance", "Plans", "Grade");
            ConvertIntGradeColumnToString(migrationBuilder, "AwardNominations", "Nominations", "Grade");
            ConvertIntGradeColumnToString(migrationBuilder, "dbo", "WorkFlows_CaseDetails", "Grade");
            ConvertIntGradeColumnToString(migrationBuilder, "Students", "SchoolEnrolments", "Grade");
            ConvertIntGradeColumnToString(migrationBuilder, "ThirdParty", "ConsentRequirements", "Grade");
            ConvertIntGradeColumnToString(migrationBuilder, "ThirdParty", "Transactions", "Grade");
            // Subjects_Courses handled separately
            ConvertSubjectsCoursesGrade(migrationBuilder);

            // Special case: sentinel value 13 meant "invalid/unknown" -> now Grade.Empty ("")
            migrationBuilder.Sql("ALTER TABLE [SciencePracs].[Lessons] ADD [Grade_New] nvarchar(3) NULL;");

            migrationBuilder.Sql(@"
                UPDATE [SciencePracs].[Lessons]
                SET [Grade_New] = CASE [Grade]
                    WHEN 5  THEN 'Y05'
                    WHEN 6  THEN 'Y06'
                    WHEN 7  THEN 'Y07'
                    WHEN 8  THEN 'Y08'
                    WHEN 9  THEN 'Y09'
                    WHEN 10 THEN 'Y10'
                    WHEN 11 THEN 'Y11'
                    WHEN 12 THEN 'Y12'
                    ELSE ''  -- old '13 = invalid/unknown' sentinel -> Grade.Empty
                END;
            ");

            migrationBuilder.Sql("ALTER TABLE [SciencePracs].[Lessons] DROP COLUMN [Grade];");
            migrationBuilder.Sql("EXEC sp_rename 'SciencePracs.Lessons.Grade_New', 'Grade', 'COLUMN';");
            migrationBuilder.Sql("ALTER TABLE [SciencePracs].[Lessons] ALTER COLUMN [Grade] nvarchar(3) NOT NULL;");

            // Special case: AwardNominations.PeriodGrades - Grade is part of the composite PK
            migrationBuilder.Sql("ALTER TABLE [AwardNominations].[PeriodGrades] DROP CONSTRAINT [PK_PeriodGrades];");
            migrationBuilder.Sql("ALTER TABLE [AwardNominations].[PeriodGrades] ADD [Grade_New] nvarchar(3) NULL;");

            migrationBuilder.Sql(@"
                UPDATE [AwardNominations].[PeriodGrades]
                SET [Grade_New] = CASE [Grade]
                    WHEN 5  THEN 'Y05'
                    WHEN 6  THEN 'Y06'
                    WHEN 7  THEN 'Y07'
                    WHEN 8  THEN 'Y08'
                    WHEN 9  THEN 'Y09'
                    WHEN 10 THEN 'Y10'
                    WHEN 11 THEN 'Y11'
                    WHEN 12 THEN 'Y12'
                    ELSE NULL
                END;
            ");

            migrationBuilder.Sql("ALTER TABLE [AwardNominations].[PeriodGrades] DROP COLUMN [Grade];");
            migrationBuilder.Sql("EXEC sp_rename 'AwardNominations.PeriodGrades.Grade_New', 'Grade', 'COLUMN';");
            migrationBuilder.Sql(
                "ALTER TABLE [AwardNominations].[PeriodGrades] ALTER COLUMN [Grade] nvarchar(3) NOT NULL;");
            migrationBuilder.Sql(@"
                ALTER TABLE [AwardNominations].[PeriodGrades]
                    ADD CONSTRAINT [PK_PeriodGrades] PRIMARY KEY CLUSTERED ([PeriodId] ASC, [Grade] ASC);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // AwardNominations.PeriodGrades first (PK column)
            migrationBuilder.Sql("ALTER TABLE [AwardNominations].[PeriodGrades] DROP CONSTRAINT [PK_PeriodGrades];");
            migrationBuilder.Sql("ALTER TABLE [AwardNominations].[PeriodGrades] ADD [Grade_Old] int NULL;");

            migrationBuilder.Sql(@"
                UPDATE [AwardNominations].[PeriodGrades]
                SET [Grade_Old] = CASE [Grade]
                    WHEN 'Y05' THEN 5  WHEN 'Y06' THEN 6  WHEN 'Y07' THEN 7  WHEN 'Y08' THEN 8
                    WHEN 'Y09' THEN 9  WHEN 'Y10' THEN 10 WHEN 'Y11' THEN 11 WHEN 'Y12' THEN 12
                    ELSE 0
                END;
            ");

            migrationBuilder.Sql("ALTER TABLE [AwardNominations].[PeriodGrades] DROP COLUMN [Grade];");
            migrationBuilder.Sql("EXEC sp_rename 'AwardNominations.PeriodGrades.Grade_Old', 'Grade', 'COLUMN';");
            migrationBuilder.Sql("ALTER TABLE [AwardNominations].[PeriodGrades] ALTER COLUMN [Grade] int NOT NULL;");
            migrationBuilder.Sql(@"
                ALTER TABLE [AwardNominations].[PeriodGrades]
                    ADD CONSTRAINT [PK_PeriodGrades] PRIMARY KEY CLUSTERED ([PeriodId] ASC, [Grade] ASC);
            ");

            // SciencePracs.Lessons: NULL reverts to the old 13 sentinel
            ConvertStringGradeColumnToInt(migrationBuilder, "SciencePracs", "Lessons", "Grade", nullSentinel: 13);
            migrationBuilder.Sql("ALTER TABLE [SciencePracs].[Lessons] ADD DEFAULT ((13)) FOR [Grade];");

            ConvertStringGradeColumnToInt(migrationBuilder, "ThirdParty", "Transactions", "Grade", nullSentinel: 0);
            ConvertStringGradeColumnToInt(migrationBuilder, "ThirdParty", "ConsentRequirements", "Grade",
                nullSentinel: 0);
            ConvertStringGradeColumnToInt(migrationBuilder, "Students", "SchoolEnrolments", "Grade", nullSentinel: 0);
            ConvertStringGradeColumnToInt(migrationBuilder, "dbo", "WorkFlows_CaseDetails", "Grade", nullSentinel: 0);
            ConvertStringGradeColumnToInt(migrationBuilder, "AwardNominations", "Nominations", "Grade",
                nullSentinel: 0);
            ConvertStringGradeColumnToInt(migrationBuilder, "Attendance", "Plans", "Grade", nullSentinel: 0);
            ConvertStringGradeColumnToInt(migrationBuilder, "Attendance", "CheckInResponses", "Grade", nullSentinel: 0);
            ConvertStringGradeColumnToInt(migrationBuilder, "Assessments", "Students", "StudentGrade", nullSentinel: 0);
        }

        private static void ConvertIntGradeColumnToString(
            MigrationBuilder migrationBuilder,
            string schema,
            string table,
            string column)
        {
            string fullTable = $"[{schema}].[{table}]";

            migrationBuilder.Sql($@"
                DECLARE @DefaultConstraint sysname;
                SELECT @DefaultConstraint = dc.name
                FROM sys.default_constraints dc
                JOIN sys.columns c ON c.object_id = dc.parent_object_id AND c.column_id = dc.parent_column_id
                WHERE dc.parent_object_id = OBJECT_ID('{schema}.{table}') AND c.name = '{column}';

                IF @DefaultConstraint IS NOT NULL
                    EXEC('ALTER TABLE {fullTable} DROP CONSTRAINT [' + @DefaultConstraint + '];');
            ");

            migrationBuilder.Sql($"ALTER TABLE {fullTable} ADD [{column}_New] nvarchar(3) NULL;");

            migrationBuilder.Sql($@"
                UPDATE {fullTable}
                SET [{column}_New] = CASE [{column}]
                    WHEN 5  THEN 'Y05'
                    WHEN 6  THEN 'Y06'
                    WHEN 7  THEN 'Y07'
                    WHEN 8  THEN 'Y08'
                    WHEN 9  THEN 'Y09'
                    WHEN 10 THEN 'Y10'
                    WHEN 11 THEN 'Y11'
                    WHEN 12 THEN 'Y12'
                    ELSE ''
                END;
            ");

            migrationBuilder.Sql($"ALTER TABLE {fullTable} DROP COLUMN [{column}];");
            migrationBuilder.Sql($"EXEC sp_rename '{schema}.{table}.{column}_New', '{column}', 'COLUMN';");
            migrationBuilder.Sql($"ALTER TABLE {fullTable} ALTER COLUMN [{column}] nvarchar(3) NOT NULL;");
        }

        private static void ConvertStringGradeColumnToInt(
            MigrationBuilder migrationBuilder,
            string schema,
            string table,
            string column,
            int nullSentinel)
        {
            string fullTable = $"[{schema}].[{table}]";

            migrationBuilder.Sql($"ALTER TABLE {fullTable} ADD [{column}_Old] int NULL;");

            migrationBuilder.Sql($@"
                UPDATE {fullTable}
                SET [{column}_Old] = CASE [{column}]
                    WHEN 'Y05' THEN 5  WHEN 'Y06' THEN 6  WHEN 'Y07' THEN 7  WHEN 'Y08' THEN 8
                    WHEN 'Y09' THEN 9  WHEN 'Y10' THEN 10 WHEN 'Y11' THEN 11 WHEN 'Y12' THEN 12
                    ELSE {nullSentinel}
                END;
            ");

            migrationBuilder.Sql($"ALTER TABLE {fullTable} DROP COLUMN [{column}];");
            migrationBuilder.Sql($"EXEC sp_rename '{schema}.{table}.{column}_Old', '{column}', 'COLUMN';");
            migrationBuilder.Sql($"ALTER TABLE {fullTable} ALTER COLUMN [{column}] int NOT NULL;");
        }

        private static void ConvertSubjectsCoursesGrade(MigrationBuilder migrationBuilder)
        {
            const string fullTable = "[dbo].[Subjects_Courses]";

            // The two long-unused non-curriculum courses being removed
            migrationBuilder.Sql(@"
                DECLARE @CourseIds TABLE (Id uniqueidentifier);
                INSERT INTO @CourseIds SELECT Id FROM [dbo].[Subjects_Courses] WHERE Grade NOT IN (5,6,7,8,9,10,11,12);

                DECLARE @OfferingIds TABLE (Id uniqueidentifier);
                INSERT INTO @OfferingIds SELECT Id FROM [dbo].[Offerings_Offerings] WHERE CourseId IN (SELECT Id FROM @CourseIds);

                DELETE FROM [dbo].[Offerings_Sessions] WHERE OfferingId IN (SELECT Id FROM @OfferingIds);
                DELETE FROM [dbo].[Offerings_Teachers] WHERE OfferingId IN (SELECT Id FROM @OfferingIds);
                DELETE FROM [dbo].[Offerings_Resources] WHERE OfferingId IN (SELECT Id FROM @OfferingIds);
                DELETE FROM [dbo].[Enrolments] WHERE OfferingId IN (SELECT Id FROM @OfferingIds);
                DELETE FROM [dbo].[Offerings_Offerings] WHERE Id IN (SELECT Id FROM @OfferingIds);
                DELETE FROM [dbo].[Subjects_Courses] WHERE Id IN (SELECT Id FROM @CourseIds);
            ");

            migrationBuilder.Sql($"ALTER TABLE {fullTable} ADD [Grade_New] nvarchar(3) NULL;");

            migrationBuilder.Sql($@"
                UPDATE {fullTable}
                SET [Grade_New] = CASE [Grade]
                    WHEN 5  THEN 'Y05'
                    WHEN 6  THEN 'Y06'
                    WHEN 7  THEN 'Y07'
                    WHEN 8  THEN 'Y08'
                    WHEN 9  THEN 'Y09'
                    WHEN 10 THEN 'Y10'
                    WHEN 11 THEN 'Y11'
                    WHEN 12 THEN 'Y12'
                END;
            ");

            migrationBuilder.Sql($"ALTER TABLE {fullTable} DROP COLUMN [Grade];");
            migrationBuilder.Sql("EXEC sp_rename 'dbo.Subjects_Courses.Grade_New', 'Grade', 'COLUMN';");
            migrationBuilder.Sql($"ALTER TABLE {fullTable} ALTER COLUMN [Grade] nvarchar(3) NOT NULL;");
        }
    }
}
