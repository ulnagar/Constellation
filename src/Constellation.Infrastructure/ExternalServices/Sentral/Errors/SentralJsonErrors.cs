namespace Constellation.Infrastructure.ExternalServices.Sentral.Errors;

using Core.Shared;

public static class SentralJsonErrors
{
    public static readonly Func<string, string, Error> IncorrectObject = (expected, provided) => new(
        "Sentral.JsonConversion.IncorrectObject",
        $"Expected object of type {expected} but received {provided}");

    public static readonly Error TooManyResponses = new(
        "Sentral.JsonConversion.TooManyResponses",
        $"Received too many objects");
}
