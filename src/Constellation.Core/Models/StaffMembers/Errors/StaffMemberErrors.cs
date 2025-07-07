namespace Constellation.Core.Models.StaffMembers.Errors;

using Constellation.Core.Shared;
using Identifiers;
using System;

public static class StaffMemberErrors
{
    public static readonly Error InvalidId = new(
        "StaffMember.InvalidId",
        "The provided staff id is not valid");

    public static readonly Func<StaffId, Error> AlreadyExists = id => new(
        "StaffMember.AlreadyExists",
        $"A staff member with the Id {id} already exists");

    public static readonly Func<int, Error> NotFoundLinkedToOffering = id => new Error(
        "StaffMember.NotFoundLinkedToOffering",
        $"Could not retrieve list of teachers for the offering {id}");

    public static readonly Func<StaffId, Error> NotFound = id => new Error(
        "StaffMember.TeacherNotFound",
        $"A teacher with the Id {id} could not be found");

    public static readonly Func<string, Error> NotFoundByEmail = email => new Error(
        "StaffMember.TeacherNotFound",
        $"A teacher with the Email Address {email} could not be found");

    public static readonly Error NoneFound = new(
        "StaffMember.NoneFound",
        "Could not find any active staff in the database");

}