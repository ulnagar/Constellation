namespace Constellation.Core.Models.Faculties.Errors;

using Constellation.Core.Shared;
using StaffMembers.Identifiers;
using System;

public static class FacultyMembershipErrors
{
    public static readonly Func<StaffId, Error> AlreadyExists = staffId => new(
        "Faculties.FacultyMembership.AlreadyExists",
        $"Staff Member {staffId} is already a member of this Faculty");

    public static readonly Func<StaffId, Error> DoesNotExist = staffId => new(
        "Faculties.FacultyMembership.DoesNotExist",
        $"Staff Member {staffId} is not a member of this Faculty");
}