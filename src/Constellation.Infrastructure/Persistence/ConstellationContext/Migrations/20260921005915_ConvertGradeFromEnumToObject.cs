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
            // Straightforward int -> nvarchar(3) columns (5-12 -> Y05-Y12, anything else -> NULL)
            ConvertIntGradeColumnToString(migrationBuilder, "Assessments", "Students", "StudentGrade",
                isNullable: false);
            ConvertIntGradeColumnToString(migrationBuilder, "Attendance", "CheckInResponses", "Grade",
                isNullable: false);
            ConvertIntGradeColumnToString(migrationBuilder, "Attendance", "Plans", "Grade", isNullable: false);
            ConvertIntGradeColumnToString(migrationBuilder, "AwardNominations", "Nominations", "Grade",
                isNullable: true);
            ConvertIntGradeColumnToString(migrationBuilder, "dbo", "Subjects_Courses", "Grade", isNullable: false);
            ConvertIntGradeColumnToString(migrationBuilder, "dbo", "WorkFlows_CaseDetails", "Grade", isNullable: true);
            ConvertIntGradeColumnToString(migrationBuilder, "Students", "SchoolEnrolments", "Grade", isNullable: false);
            ConvertIntGradeColumnToString(migrationBuilder, "ThirdParty", "ConsentRequirements", "Grade",
                isNullable: true);
            ConvertIntGradeColumnToString(migrationBuilder, "ThirdParty", "Transactions", "Grade", isNullable: false);

            // Special case: sentinel value 13 meant "invalid/unknown" -> now Grade.Empty, which the
            // converter round-trips as NULL (GradeConverter.GradeToString maps Grade.Empty -> null)
            migrationBuilder.Sql(@"
                DECLARE @DefaultConstraint sysname;
                SELECT @DefaultConstraint = dc.name
                FROM sys.default_constraints dc
                JOIN sys.columns c ON c.object_id = dc.parent_object_id AND c.column_id = dc.parent_column_id
                WHERE dc.parent_object_id = OBJECT_ID('SciencePracs.Lessons') AND c.name = 'Grade';

                IF @DefaultConstraint IS NOT NULL
                    EXEC('ALTER TABLE [SciencePracs].[Lessons] DROP CONSTRAINT [' + @DefaultConstraint + '];');
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE [SciencePracs].[Lessons] ADD [Grade_New] nvarchar(3) NULL;

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
                    ELSE NULL  -- covers the old '13 = invalid/unknown' sentinel
                END;

                ALTER TABLE [SciencePracs].[Lessons] DROP COLUMN [Grade];
            ");

            migrationBuilder.Sql("EXEC sp_rename 'SciencePracs.Lessons.Grade_New', 'Grade', 'COLUMN';");

            // NOT NULL with no default: Grade.Empty (NULL) is a valid, meaningful value here, so the
            // column stays nullable rather than forcing a non-null default.
            migrationBuilder.Sql("ALTER TABLE [SciencePracs].[Lessons] ALTER COLUMN [Grade] nvarchar(3) NULL;");

            // Special case: AwardNominations.PeriodGrades - Grade is part of the composite PK
            migrationBuilder.Sql(@"
                ALTER TABLE [AwardNominations].[PeriodGrades] DROP CONSTRAINT [PK_PeriodGrades];

                ALTER TABLE [AwardNominations].[PeriodGrades] ADD [Grade_New] nvarchar(3) NULL;

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

                ALTER TABLE [AwardNominations].[PeriodGrades] DROP COLUMN [Grade];
            ");

            migrationBuilder.Sql("EXEC sp_rename 'AwardNominations.PeriodGrades.Grade_New', 'Grade', 'COLUMN';");

            migrationBuilder.Sql(@"
                ALTER TABLE [AwardNominations].[PeriodGrades] ALTER COLUMN [Grade] nvarchar(3) NOT NULL;

                ALTER TABLE [AwardNominations].[PeriodGrades]
                    ADD CONSTRAINT [PK_PeriodGrades] PRIMARY KEY CLUSTERED ([PeriodId] ASC, [Grade] ASC);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Reverse: AwardNominations.PeriodGrades first (PK column)
            migrationBuilder.Sql(@"
                ALTER TABLE [AwardNominations].[PeriodGrades] DROP CONSTRAINT [PK_PeriodGrades];

                ALTER TABLE [AwardNominations].[PeriodGrades] ADD [Grade_Old] int NULL;

                UPDATE [AwardNominations].[PeriodGrades]
                SET [Grade_Old] = CASE [Grade]
                    WHEN 'Y05' THEN 5  WHEN 'Y06' THEN 6  WHEN 'Y07' THEN 7  WHEN 'Y08' THEN 8
                    WHEN 'Y09' THEN 9  WHEN 'Y10' THEN 10 WHEN 'Y11' THEN 11 WHEN 'Y12' THEN 12
                    ELSE 0
                END;

                ALTER TABLE [AwardNominations].[PeriodGrades] DROP COLUMN [Grade];
            ");

            migrationBuilder.Sql("EXEC sp_rename 'AwardNominations.PeriodGrades.Grade_Old', 'Grade', 'COLUMN';");

            migrationBuilder.Sql(@"
                ALTER TABLE [AwardNominations].[PeriodGrades] ALTER COLUMN [Grade] int NOT NULL;

                ALTER TABLE [AwardNominations].[PeriodGrades]
                    ADD CONSTRAINT [PK_PeriodGrades] PRIMARY KEY CLUSTERED ([PeriodId] ASC, [Grade] ASC);
            ");

            // SciencePracs.Lessons: NULL reverts to the old 13 sentinel
            ConvertStringGradeColumnToInt(migrationBuilder, "SciencePracs", "Lessons", "Grade", isNullable: false,
                nullSentinel: 13);
            migrationBuilder.Sql("ALTER TABLE [SciencePracs].[Lessons] ADD DEFAULT ((13)) FOR [Grade];");

            ConvertStringGradeColumnToInt(migrationBuilder, "ThirdParty", "Transactions", "Grade", isNullable: false,
                nullSentinel: 0);
            ConvertStringGradeColumnToInt(migrationBuilder, "ThirdParty", "ConsentRequirements", "Grade",
                isNullable: true, nullSentinel: null);
            ConvertStringGradeColumnToInt(migrationBuilder, "Students", "SchoolEnrolments", "Grade", isNullable: false,
                nullSentinel: 0);
            ConvertStringGradeColumnToInt(migrationBuilder, "dbo", "WorkFlows_CaseDetails", "Grade", isNullable: true,
                nullSentinel: null);
            ConvertStringGradeColumnToInt(migrationBuilder, "dbo", "Subjects_Courses", "Grade", isNullable: false,
                nullSentinel: 0);
            ConvertStringGradeColumnToInt(migrationBuilder, "AwardNominations", "Nominations", "Grade",
                isNullable: true, nullSentinel: null);
            ConvertStringGradeColumnToInt(migrationBuilder, "Attendance", "Plans", "Grade", isNullable: false,
                nullSentinel: 0);
            ConvertStringGradeColumnToInt(migrationBuilder, "Attendance", "CheckInResponses", "Grade",
                isNullable: false, nullSentinel: 0);
            ConvertStringGradeColumnToInt(migrationBuilder, "Assessments", "Students", "StudentGrade",
                isNullable: false, nullSentinel: 0);
        }

        private static void ConvertIntGradeColumnToString(
            MigrationBuilder migrationBuilder,
            string schema,
            string table,
            string column,
            bool isNullable)
        {
            string fullTable = $"[{schema}].[{table}]";

            // Drop any default constraint on the column first
            migrationBuilder.Sql($@"
                DECLARE @DefaultConstraint sysname;
                SELECT @DefaultConstraint = dc.name
                FROM sys.default_constraints dc
                JOIN sys.columns c ON c.object_id = dc.parent_object_id AND c.column_id = dc.parent_column_id
                WHERE dc.parent_object_id = OBJECT_ID('{schema}.{table}') AND c.name = '{column}';

                IF @DefaultConstraint IS NOT NULL
                    EXEC('ALTER TABLE {fullTable} DROP CONSTRAINT [' + @DefaultConstraint + '];');
            ");

            migrationBuilder.Sql($@"
                ALTER TABLE {fullTable} ADD [{column}_New] nvarchar(3) NULL;

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
                    ELSE NULL
                END;

                ALTER TABLE {fullTable} DROP COLUMN [{column}];
            ");

            migrationBuilder.Sql($"EXEC sp_rename '{schema}.{table}.{column}_New', '{column}', 'COLUMN';");

            if (isNullable)
            {
                migrationBuilder.Sql($"ALTER TABLE {fullTable} ALTER COLUMN [{column}] nvarchar(3) NULL;");
            }
            else
            {
                // NOTE: if any row resolved to NULL above (an unexpected/out-of-range old value),
                // this ALTER COLUMN will fail. Investigate and clean those rows before rerunning
                // if that happens - do not silently coerce them to Grade.Empty here.
                migrationBuilder.Sql($"ALTER TABLE {fullTable} ALTER COLUMN [{column}] nvarchar(3) NOT NULL;");
            }
        }

        private static void ConvertStringGradeColumnToInt(
            MigrationBuilder migrationBuilder,
            string schema,
            string table,
            string column,
            bool isNullable,
            int? nullSentinel)
        {
            string fullTable = $"[{schema}].[{table}]";
            string elseClause = nullSentinel.HasValue ? nullSentinel.Value.ToString() : "NULL";

            migrationBuilder.Sql($@"
                ALTER TABLE {fullTable} ADD [{column}_Old] int NULL;

                UPDATE {fullTable}
                SET [{column}_Old] = CASE [{column}]
                    WHEN 'Y05' THEN 5  WHEN 'Y06' THEN 6  WHEN 'Y07' THEN 7  WHEN 'Y08' THEN 8
                    WHEN 'Y09' THEN 9  WHEN 'Y10' THEN 10 WHEN 'Y11' THEN 11 WHEN 'Y12' THEN 12
                    ELSE {elseClause}
                END;

                ALTER TABLE {fullTable} DROP COLUMN [{column}];
            ");

            migrationBuilder.Sql($"EXEC sp_rename '{schema}.{table}.{column}_Old', '{column}', 'COLUMN';");

            migrationBuilder.Sql(isNullable
                ? $"ALTER TABLE {fullTable} ALTER COLUMN [{column}] int NULL;"
                : $"ALTER TABLE {fullTable} ALTER COLUMN [{column}] int NOT NULL;");
        }
    }
}
