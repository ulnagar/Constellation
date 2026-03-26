namespace Constellation.Application.Domains.Messaging.History.Queries.GetRecentCommunicationsHistory;

using Abstractions.Messaging;
using Constellation.Core.Models.Messaging.Email.Enums;
using Constellation.Core.Models.Messaging.Email.Repositories;
using Constellation.Core.Models.Messaging.Enums;
using Constellation.Core.Models.Messaging.Sms.Repositories;
using Core.Models.Messaging.Email;
using Core.Models.Messaging.Sms;
using Core.Primitives;
using Core.Shared;
using Core.ValueObjects;
using Models;
using System.Collections.Generic;

internal sealed class GetRecentCommunicationsHistoryQueryHandler
: IQueryHandler<GetRecentCommunicationsHistoryQuery, List<CommunicationRecordResponse>>
{
    private readonly IEmailRepository _emailRepository;
    private readonly ISmsRepository _smsRepository;

    public GetRecentCommunicationsHistoryQueryHandler(
        IEmailRepository emailRepository,
        ISmsRepository smsRepository)
    {
        _emailRepository = emailRepository;
        _smsRepository = smsRepository;
    }

    public async Task<Result<List<CommunicationRecordResponse>>> Handle(GetRecentCommunicationsHistoryQuery request, CancellationToken cancellationToken)
    {
        List<EmailMessage> emails = await _emailRepository.GetRecent(request.Limit, cancellationToken);
        List<SmsMessage> smsMessages = await _smsRepository.GetRecent(request.Limit, cancellationToken);

        List<IHasCreatedAt> mostRecent = emails.Cast<IHasCreatedAt>()
            .Concat(smsMessages)
            .OrderByDescending(entry => entry.CreatedAt)
            .Take(request.Limit)
            .ToList();

        List<CommunicationRecordResponse> responses = [];
        
        foreach (IHasCreatedAt message in mostRecent)
        {
            if (message is EmailMessage email)
            {
                List<CommunicationRecordResponse.Recipient> recipients = [];

                foreach (EmailMessageRecipient recipient in email.Recipients)
                {
                    recipients.Add(new(
                        recipient.RecipientType,
                        recipient.Recipient.Name,
                        recipient.Recipient.Email));
                }

                responses.Add(new(
                    email.Id,
                    MessageType.Email,
                    MessageDirection.Outbound,
                    $"{email.From.Name} <{email.From.Email}>",
                    recipients,
                    email.Subject,
                    email.Status,
                    email.CreatedAt));
            }

            if (message is SmsMessage sms)
            {
                List<CommunicationRecordResponse.Recipient> recipients =
                [
                    new(
                        EmailRecipientType.To,
                        sms.Recipient.Name,
                        sms.Recipient.Number)
                ];

                responses.Add(new(
                    sms.Id,
                    MessageType.SMS,
                    sms.Direction,
                    sms.Sender.Name,
                    recipients,
                    sms.Message,
                    sms.Status,
                    sms.CreatedAt));
            }
        }

        return responses;
    }
}
