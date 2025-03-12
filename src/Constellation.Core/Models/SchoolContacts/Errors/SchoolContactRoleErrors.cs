namespace Constellation.Core.Models.SchoolContacts.Errors;

using Enums;
using Identifiers;
using Shared;
using System;

public static class SchoolContactRoleErrors
{
    public static readonly Func<SchoolContactRoleId, Error> NotFound = id => new(
        "SchoolContactRole.NotFound",
        $"Could not find a School Contact Role with the Id {id}");

    public static readonly Func<string, Error> NotFoundForSchool = id => new(
        "SchoolContactRole.NotFoundForSchool",
        $"Could not find a School Contact Role for School with Id {id}");

    public static readonly Func<string, Position, Error> NotFoundForSchoolAndRole = (school, role) => new(
        "SchoolContactRole.NotFoundForSchoolAndRole",
        $"Could not find a {role} registered for school {school}");

    public static class Validation
    {
        public static readonly Error RoleEmpty = new(
            "SchoolContactRole.Validation.RoleEmpty",
            "The provided Role name is empty");

        public static readonly Error SchoolCodeEmpty = new(
            "SchoolContactRole.Validation.SchoolCodeEmpty",
            "The provided School Code is empty");
    }
}