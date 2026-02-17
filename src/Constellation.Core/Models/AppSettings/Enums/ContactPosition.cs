namespace Constellation.Core.Models.AppSettings.Enums;

using Common;

public sealed class ContactPosition : StringEnumeration<ContactPosition>
{
    public static readonly ContactPosition Counsellor = new("Counsellor", "Counsellor");
    public static readonly ContactPosition CareersAdvisor = new("CareersAdvisor", "Careers Advisor");
    public static readonly ContactPosition Librarian = new("Librarian", "Librarian");
    public static readonly ContactPosition InstructionalLeader = new("InstructionalLeader", "Instructional Leader");
    public static readonly ContactPosition LearningSupport = new("LearningSupport", "Learning and Support Teacher");
    public static readonly ContactPosition DeputyPrincipal = new("DeputyPrincipal", "Deputy Principal");
    public static readonly ContactPosition Principal = new("Principal", "Principal");

    private ContactPosition(string value, string name) 
        : base(value, name)
    {
    }
}