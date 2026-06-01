namespace Constellation.Core.Models.Faculties.Enums;

using Core.Common;

public sealed class FacultyMembershipRole : StringEnumeration<FacultyMembershipRole>
{
    public static readonly FacultyMembershipRole Empty = new(string.Empty);

    public static readonly FacultyMembershipRole Member = new("Member");
    public static readonly FacultyMembershipRole Approver = new("Approver");
    public static readonly FacultyMembershipRole Manager = new("Manager");

    private FacultyMembershipRole(string value)
        : base(value, value) { }

    public static IEnumerable<FacultyMembershipRole> GetOptions => GetEnumerable;
}