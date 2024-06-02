namespace Constellation.Core.Models.Faculty.Errors;

using Identifiers;
using Shared;
using System;

public static class FacultyErrors
{
    public static readonly Func<FacultyId, Error> NotFound = id => new(
        "Faculties.Faculty.NotFound",
        $"Could not find a Faculty with the Id {id}");

    public static readonly Func<string, Error> NotFoundForStaff = id => new(
        "Faculties.Faculty.NotFoundForStaff",
        $"Could not find any linked Faculties for Staff Member with Id {id}");
}