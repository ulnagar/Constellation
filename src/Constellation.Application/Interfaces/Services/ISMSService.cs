namespace Constellation.Application.Interfaces.Services;

using Constellation.Core.Models.Students;
using Constellation.Core.ValueObjects;
using Core.Shared;
using Domains.Messaging.Sms.Dtos;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface ISMSService
{
    Task<Result<List<OutgoingSmsConfirmation>>> SendAbsenceNotification(DateOnly absenceDate, Student student, List<SmsRecipient> recipients, CancellationToken cancellationToken = default);
    Task<Result> SendQueuedMessage(MessageSender sender, SmsRecipient receiver, string messageBody, CancellationToken cancellationToken = default);
    Task<Result> SendLoginToken(string token, PhoneNumber phoneNumber, CancellationToken cancellationToken = default);
}
