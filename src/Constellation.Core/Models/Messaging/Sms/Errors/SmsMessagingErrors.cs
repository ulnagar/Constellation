namespace Constellation.Core.Models.Messaging.Sms.Errors;

using Identifiers;
using Shared;

public static class SmsMessagingErrors
{
    public static readonly Func<SmsId, Error> NotFound = id => new(
        "Messaging.SMS.NotFound",
        $"Could not find an SMS with the Id {id}");

    public static readonly Error DeliveryReceiptIncomplete = new(
        "Messaging.SMS.DeliveryReceiptIncomplete",
        "The SMS Delivery Receipt is incomplete");
}
