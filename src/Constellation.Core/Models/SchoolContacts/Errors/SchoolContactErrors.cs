namespace Constellation.Core.Models.SchoolContacts.Errors;

using Identifiers;
using Shared;
using System;

public static class SchoolContactErrors
{
    public static readonly Func<SchoolContactId, Error> NotFound = id => new(
        "SchoolContact.NotFound",
        $"Could not find a School Contact with the Id {id}");

    public static readonly Func<string, Error> NotFoundByName = name => new(
        "SchoolContact.NotFound",
        $"Could not find a School Contact with the name {name}");

    public static class Validation
    {
        public static readonly Error FirstNameEmpty = new(
            "SchoolContact.Validation.FirstNameEmpty",
            "The provided First Name is empty");

        public static readonly Error LastNameEmpty = new(
            "SchoolContact.Validation.LastNameEmpty",
            "The provided Last Name is empty");

        public static readonly Error EmailAddressEmpty = new(
            "SchoolContact.Validation.EmailAddressEmpty",
            "The provided email address is empty");

        public static readonly Error PhoneNumberInvalid = new(
            "SchoolContact.Validation.PhoneNumberInvalid",
            "The provided Phone Number is not valid");
    }
}

public static class SchoolContactRoleErrors
{
    public static readonly Func<SchoolContactRoleId, Error> NotFound = id => new(
        "SchoolContactRole.NotFound",
        $"Could not find a School Contact Role with the Id {id}");

    public static readonly Func<string, Error> NotFoundForSchool = id => new(
        "SchoolContactRole.NotFoundForSchool",
        $"Could not find a School Contact Role for School with Id {id}");

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