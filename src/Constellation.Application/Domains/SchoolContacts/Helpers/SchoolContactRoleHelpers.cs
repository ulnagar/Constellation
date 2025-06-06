﻿namespace Constellation.Application.Domains.SchoolContacts.Helpers;

using Core.Models.SchoolContacts;

public static class SchoolContactRoleHelpers
{
    public static bool IsContactRoleRestricted(this SchoolContactRole assignment) 
        => assignment.Role.IsRestricted;
}