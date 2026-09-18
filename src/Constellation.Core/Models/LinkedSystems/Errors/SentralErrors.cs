namespace Constellation.Core.Models.LinkedSystems.Errors;

using Shared;

public static class SentralErrors
{
    public static readonly Func<string, Error> FamilyIdNotValid = id => new Error(
        "LinkedSystems.Sentral.FamilyIdNotValid",
        $"The value {id} is not a valid Sentral Family Id.");
}