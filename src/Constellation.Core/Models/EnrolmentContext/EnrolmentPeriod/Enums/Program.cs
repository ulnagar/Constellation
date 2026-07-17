namespace Constellation.Core.Models.EnrolmentContext.EnrolmentPeriod.Enums;

using Common;

public sealed class Program : StringEnumeration<Program>
{
    public static readonly Program Empty = new(string.Empty, string.Empty);

    public static readonly Program OpportunityClass = new("OC", "Opportunity Class");
    public static readonly Program SelectiveHighSchool = new("SHS", "Selective High School");
    public static readonly Program YoungAndDeadlyMob = new("YDM", "Young and Deadly Mob");
    public static readonly Program StageSixRural = new("S6R", "Stage 6 Rural and Remote");
    public static readonly Program StageSixMetro = new("S6M", "Stage 6 Metropolitan");

    public Program(string value, string name)
        : base(value, name) { }

    public static IEnumerable<Program> GetOptions => GetEnumerable;
}