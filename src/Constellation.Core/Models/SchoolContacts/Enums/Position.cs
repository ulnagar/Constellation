namespace Constellation.Core.Models.SchoolContacts.Enums;

using Constellation.Core.Common;
using System.Collections.Generic;

public sealed class Position : StringEnumeration<Position>
{
    public static readonly Position Empty = new("", true, 99);

    public static readonly Position Principal = new("Principal", true, 2);
    public static readonly Position DirectorEducationalLeadership = new("Dir. Educational Leadership", true, 1);
    public static readonly Position Coordinator = new("Aurora College Coordinator", false, 4);
    public static readonly Position SciencePracticalTeacher = new("Science Practical Teacher", false, 5);
    public static readonly Position TimetableOfficer = new("Timetable Officer", true, 2);
    public static readonly Position NESACoordinator = new("NESA Coordinator", true, 3);
    public static readonly Position TechnologyOfficer = new("Technology Support Officer", true, 6);

    public bool IsRestricted { get; init; }
    public int SortOrder { get; init; }

    private Position(string value, bool restricted, int order)
        : base(value, value)
    {
        IsRestricted = restricted;
        SortOrder = order;
    }

    public static IEnumerable<Position> GetOptions => GetEnumerable;
}