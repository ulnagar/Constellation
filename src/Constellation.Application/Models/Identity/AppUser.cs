namespace Constellation.Application.Models.Identity;

using Constellation.Core.ValueObjects;
using Core.Models.SchoolContacts.Identifiers;
using Core.Models.Students.Identifiers;
using Microsoft.AspNetCore.Identity;
using System;

public class AppUser : IdentityUser<Guid>
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string DisplayName => GetDisplayName();
    public bool IsSchoolContact { get; set; }
    public SchoolContactId SchoolContactId { get; set; }
    public bool IsStaffMember { get; set; }
    public string StaffId { get; set; }
    public bool IsParent { get; set; }
    public bool IsStudent { get; set; }
    public StudentId StudentId { get; set; }
    public DateTime? LastLoggedIn { get; set; }

    private string GetDisplayName()
    {
        var name = Name.Create(FirstName, null, LastName);

        if (name.IsFailure)
            return $"{FirstName} {LastName}";

        return name.Value.DisplayName;
    }
}
