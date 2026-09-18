namespace Constellation.Core.Models.Absences.Errors;

using Identifiers;
using Shared;

public static class AbsenceResponseErrors
{
    public static readonly Func<AbsenceResponseId, Error> NotFound = id => new Error(
        "Absences.Response.NotFound",
        $"Could not find any response with the id {id}");
}