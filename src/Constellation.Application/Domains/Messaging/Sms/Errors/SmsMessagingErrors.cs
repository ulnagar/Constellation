namespace Constellation.Application.Domains.Messaging.Sms.Errors;

using Core.Shared;

public static class SmsMessagingErrors
{
    public static readonly Error DeliveryReceiptIncomplete = new(
        "Messaging.SMS.DeliveryReceiptIncomplete",
        "The SMS Delivery Receipt is incomplete");
}
