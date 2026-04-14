namespace Constellation.Core.Models.Auth;

using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.SchoolContacts.Identifiers;
using Constellation.Core.Models.StaffMembers.Identifiers;
using Constellation.Core.Models.Students.Identifiers;
using Constellation.Core.Primitives;
using Enums;

public sealed class AppUserLink : IAuditableEntity
{
    private AppUserLink() { }

    public AppUserLink(StaffId staffId, Guid appUserId)
    {
        Id = Guid.NewGuid();
        AppUserId = appUserId;
        Type = LinkType.Staff;
        LinkId = staffId.Value;
    }

    public AppUserLink(StudentId studentId, Guid appUserId)
    {
        Id = Guid.NewGuid();
        AppUserId = appUserId;
        Type = LinkType.Student;
        LinkId = studentId.Value;
    }

    public AppUserLink(ParentId parentId, Guid appUserId)
    {
        Id = Guid.NewGuid();
        AppUserId = appUserId;
        Type = LinkType.Parent;
        LinkId = parentId.Value;
    }

    public AppUserLink(FamilyId familyId, Guid appUserId)
    {
        Id = Guid.NewGuid();
        AppUserId = appUserId;
        Type = LinkType.Family;
        LinkId = familyId.Value;
    }

    public AppUserLink(SchoolContactId contactId, Guid appUserId)
    {
        Id = Guid.NewGuid();
        AppUserId = appUserId;
        Type = LinkType.Contact;
        LinkId = contactId.Value;
    }

    public Guid Id { get; init; }
    public Guid AppUserId { get; private set; }
    public LinkType Type { get; private set; }
    public Guid LinkId { get; private set; }

    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime ModifiedAt { get; set; }
    public bool IsDeleted { get; private set; }
    public string? DeletedBy { get; set; }
    public DateTime DeletedAt { get; set; }

    public void Delete() => IsDeleted = true;
}