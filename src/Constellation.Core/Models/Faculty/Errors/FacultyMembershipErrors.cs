namespace Constellation.Core.Models.Faculty.Errors;

using Shared;
using System;

public static class FacultyMembershipErrors
{
    public static readonly Func<string, Error> AlreadyExists = staffId => new(
        "Faculties.FacultyMembership.AlreadyExists",
        $"Staff Member {staffId} is already a member of this Faculty");

    public static readonly Func<string, Error> DoesNotExist = staffId => new(
        "Faculties.FacultyMembership.DoesNotExist",
        $"Staff Member {staffId} is not a member of this Faculty");
}