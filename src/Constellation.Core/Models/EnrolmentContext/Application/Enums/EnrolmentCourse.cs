namespace Constellation.Core.Models.EnrolmentContext.Application.Enums;

using Common;

public sealed class EnrolmentCourse : StringEnumeration<EnrolmentCourse>
{
    public static readonly EnrolmentCourse Empty = new(string.Empty, string.Empty);

    public static readonly EnrolmentCourse AboriginalStudies = new("ABS", "Aboriginal Studies");
    public static readonly EnrolmentCourse Agriculture = new("AGR", "Agriculture");
    public static readonly EnrolmentCourse Biology = new("BIO", "Biology");
    public static readonly EnrolmentCourse Chemistry = new("CHE", "Chemistry");
    public static readonly EnrolmentCourse EarthEnvironmentScience = new("EES", "Earth and Environmental Science");
    public static readonly EnrolmentCourse Economics = new("ECO", "Economics");
    public static readonly EnrolmentCourse EnglishAdv = new("ENA", "English Advanced");
    public static readonly EnrolmentCourse EnglishExt = new("ENX", "English Extension 1");
    public static readonly EnrolmentCourse EnglishExt2 = new("EXX", "English Extension 2");
    public static readonly EnrolmentCourse HistoryExt = new("HIX", "History Extension");
    public static readonly EnrolmentCourse LegalStudies = new("LEG", "Legal Studies");
    public static readonly EnrolmentCourse MathsAdv = new("MAA", "Mathematics Advanced");
    public static readonly EnrolmentCourse MathsExt = new("MAX", "Mathematics Extension 1");
    public static readonly EnrolmentCourse MathsExt2 = new("MXX", "Mathematics Extension 2");
    public static readonly EnrolmentCourse ModernHistory = new("MOD", "Modern History");
    public static readonly EnrolmentCourse Physics = new("PHY", "Physics");
    public static readonly EnrolmentCourse ScienceExt = new("SCX", "Science Extension");
    public static readonly EnrolmentCourse SoftwareEng = new("SWE", "Software Engineering");

    public EnrolmentCourse(string value, string name)
        : base(value, name) { }

    public static IEnumerable<EnrolmentCourse> GetOptions => GetEnumerable;
}