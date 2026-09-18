namespace Constellation.Core.Models.Absences.Errors;

using Constellation.Core.Models.Absences.Identifiers;
using Shared;
using System;

public static class AbsenceErrors
{
    public static readonly Error AlreadyExplained = new(
        "Absences.Absence.AlreadyExplained",
        "Cannot explain an absence that has already been explained");

    public static readonly Func<AbsenceId, Error> NotFound = id => new Error(
        "Absences.Absence.NotFound",
        $"Could not find any absence with the id {id}");
}