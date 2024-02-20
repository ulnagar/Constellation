namespace Constellation.Application.SchoolContacts.GetContactsWithRoleFromSchool;

using Constellation.Core.Models.SchoolContacts;
using Core.Models.SchoolContacts.Identifiers;

public sealed record ContactResponse(
    SchoolContactId ContactId,
    SchoolContactRoleId AssignmentId,
    string FirstName,
    string LastName,
    string PhoneNumber,
    string EmailAddress,
    string Position)
{
    public int PositionSort() =>
        Position switch
        {
            SchoolContactRole.Principal => 1,
            SchoolContactRole.Coordinator => 2,
            SchoolContactRole.SciencePrac => 3,
            _ => 10
        };
}