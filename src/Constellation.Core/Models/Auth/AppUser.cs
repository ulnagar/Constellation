namespace Constellation.Core.Models.Auth;

using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.SchoolContacts.Identifiers;
using Constellation.Core.Models.StaffMembers.Identifiers;
using Constellation.Core.Models.Students.Identifiers;
using Constellation.Core.ValueObjects;
using Enums;
using Microsoft.AspNetCore.Identity;
using System;

public sealed class AppUser : IdentityUser<Guid>
{
    private readonly List<AppUserLoginAttempt> _logins = [];
    private readonly List<AppUserLink> _links = [];

    public Name Name { get; set; }

    public bool IsSchoolContact => _links.Any(link => !link.IsDeleted && link.Type == LinkType.Contact);
    public bool IsStaffMember => _links.Any(link => !link.IsDeleted && link.Type == LinkType.Staff);
    public bool IsParent => _links.Any(link => !link.IsDeleted && link.Type == LinkType.Parent);
    public bool IsFamily => _links.Any(link => !link.IsDeleted && link.Type == LinkType.Family);
    public bool IsStudent => _links.Any(link => !link.IsDeleted && link.Type == LinkType.Student);

    public IReadOnlyList<AppUserLoginAttempt> Logins => _logins.AsReadOnly();
    public IReadOnlyList<AppUserLink> Links => _links.AsReadOnly();

    public void AddLogin(DateTime dateTime, LoginStatus status) 
        => _logins.Add(new(Id, dateTime, status));

    public void AddStudentLink(StudentId studentId)
        => _links.Add(new(studentId, Id));

    public void AddStaffLink(StaffId staffId)
        => _links.Add(new(staffId, Id));

    public void AddParentLink(ParentId parentId)
        => _links.Add(new(parentId, Id));

    public void AddFamilyLink(FamilyId familyId)
        => _links.Add(new(familyId, Id));

    public void AddContactLink(SchoolContactId contactId)
        => _links.Add(new(contactId, Id));
}