namespace Constellation.Application.Domains.Messaging.Sms.Commands.RecordSmsDeliveryReceipt;

using Abstractions.Messaging;
using Dtos;

public sealed record RecordSmsDeliveryReceiptCommand(
    SmsDeliveryReceipt Receipt)
    : ICommand;