namespace Constellation.Infrastructure.ExternalServices.SMS.Errors;

using Core.Shared;

public static class SMSGatewayErrors
{

    public static readonly Error IncorrectResponseFromServer = new(
        "SMS.IncorrectResponseFromServer",
        "The Server responded with an unexpected result");
}
