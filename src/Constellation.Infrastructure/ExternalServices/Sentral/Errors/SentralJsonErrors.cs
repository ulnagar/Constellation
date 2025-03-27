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

public static class SentralGatewayErrors
{
    public static readonly Error NoStudentIdsProvided = new(
        "Sentral.NoStudentIdsProvided",
        "This action requires selected Student Ids to complete");

    public static readonly Error IncorrectResponseFromServer = new(
        "Sentral.IncorrectResponseFromServer",
        "The Sentral Server responded with an unexpected result");
}