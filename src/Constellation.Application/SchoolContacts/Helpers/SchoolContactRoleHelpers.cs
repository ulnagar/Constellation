namespace Constellation.Application.SchoolContacts.Helpers;

using Core.Models.SchoolContacts;

public static class SchoolContactRoleHelpers
{
    public static bool IsContactRoleRestricted(this SchoolContactRole assignment)
    {
        return assignment.Role != SchoolContactRole.Coordinator && assignment.Role != SchoolContactRole.SciencePrac;
    }
}