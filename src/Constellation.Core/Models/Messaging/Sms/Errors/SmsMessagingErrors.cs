namespace Constellation.Core.Models.Messaging.Sms.Errors;

using Shared;

public static class SmsMessagingErrors
{
    public static readonly Error DeliveryReceiptIncomplete = new(
        "Messaging.SMS.DeliveryReceiptIncomplete",
        "The SMS Delivery Receipt is incomplete");
}
