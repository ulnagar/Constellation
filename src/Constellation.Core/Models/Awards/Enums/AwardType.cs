namespace Constellation.Core.Models.Awards.Enums;

using Common;
using Core.Enums;

public sealed class AwardType : StringEnumeration<AwardType>
{
    public static readonly AwardType Empty = new(string.Empty);

    public static readonly AwardType FirstInSubject = new("First in Course");
    public static readonly AwardType FirstInSubjectMathematics = new("First in Course - Mathematics", [Grade.Y05, Grade.Y06]);
    public static readonly AwardType FirstInSubjectScienceTechnology = new("First in Course - Science & Technology", [Grade.Y05, Grade.Y06]);
    public static readonly AwardType AcademicAchievement = new("Academic Achievement");
    public static readonly AwardType AcademicAchievementMathematics = new("Academic Achievement - Mathematics", [Grade.Y05, Grade.Y06]);
    public static readonly AwardType AcademicAchievementScienceTechnology = new("Academic Achievement - Science & Technology", [Grade.Y05, Grade.Y06]);
    public static readonly AwardType AcademicExcellence = new("Academic Excellence");
    public static readonly AwardType AcademicExcellenceMathematics = new("Academic Excellence - Mathematics", [Grade.Y05, Grade.Y06]);
    public static readonly AwardType AcademicExcellenceScienceTechnology = new("Academic Excellence - Science & Technology", [Grade.Y05, Grade.Y06]);
    public static readonly AwardType PrincipalsAward = new("Principals Award");
    public static readonly AwardType GalaxyMedal = new("Galaxy Medal");
    public static readonly AwardType UniversalAchiever = new("Universal Achiever");

    public static new AwardType FromValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Empty;

        IEnumerable<AwardType> defined = GetEnumerable
            .Where(entry => entry.Value == value)
            .ToList();

        if (!defined.Any() || defined.Count() > 1)
            return Empty;

        return defined.First();
    }

    private AwardType(string value)
        : base(value, value)
    {
        Value = value;
        Grades = new();
    }

    private AwardType(string value, List<Grade> grades)
        : base(value, value)
    {
        Value = value;
        Grades = grades;
    }

    public List<Grade> Grades { get; }
    
    public static IEnumerable<AwardType> GetOptions => GetEnumerable;
}