namespace Constellation.Core.Models.Faculty;

using Constellation.Core.Primitives;
using Identifiers;
using System;
using ValueObjects;

public sealed class FacultyMembership : IAuditableEntity
{
    private FacultyMembership(
        string staffId,
        FacultyId facultyId,
        FacultyMembershipRole role)
    {
        Id = new();
        StaffId = staffId;
        FacultyId = facultyId;
        Role = role;
    }

    public FacultyMembershipId Id { get; private set; }
    public string StaffId { get; private set; }
    public FacultyId FacultyId { get; private set; }
    public FacultyMembershipRole Role { get; private set; }
    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string ModifiedBy { get; set; }
    public DateTime ModifiedAt { get; set; }
    public bool IsDeleted { get; private set; }
    public string DeletedBy { get; set; }
    public DateTime DeletedAt { get; set; }

    internal static FacultyMembership Create(
        string staffId,
        FacultyId facultyId,
        FacultyMembershipRole role)
    {
        FacultyMembership membership = new(
            staffId,
            facultyId,
            role);

        return membership;
    }

    internal void Delete() => IsDeleted = true;
}
