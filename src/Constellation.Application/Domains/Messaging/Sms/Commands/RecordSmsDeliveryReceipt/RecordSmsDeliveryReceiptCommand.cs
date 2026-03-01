namespace Constellation.Application.Domains.Messaging.Sms.Commands.RecordSmsDeliveryReceipt;

using Abstractions.Messaging;
using Presentation.Server.Areas.API.Models;

public sealed record RecordSmsDeliveryReceiptCommand(
    SmsDeliveryReceipt Receipt)
    : ICommand;