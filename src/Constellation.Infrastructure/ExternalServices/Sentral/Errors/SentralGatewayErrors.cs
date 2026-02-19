namespace Constellation.Infrastructure.ExternalServices.Sentral.Errors;

using Core.Shared;

public static class SentralGatewayErrors
{
    public static readonly Error NoStudentIdsProvided = new(
        "Sentral.NoStudentIdsProvided",
        "This action requires selected Student Ids to complete");

    public static readonly Error IncorrectResponseFromServer = new(
        "Sentral.IncorrectResponseFromServer",
        "The Sentral Server responded with an unexpected result");

    public static readonly Func<string, Error> DataConversionError = item => new(
        "Sentral.DataConversionError",
        $"Failed to convert the {item} item to a value Sentral could understand");

    public static readonly Error TermDatesNotFound = new(
        "SentralGateway.GetWeekForDate.NotFound",
        "Could not identify Term and Week from date");
}