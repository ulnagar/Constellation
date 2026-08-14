namespace Constellation.Application.Domains.Messaging.History.Queries.GetMessageDetails;

using Abstractions.Messaging;
using Core.Models.Messaging.Email;
using Core.Models.Messaging.Email.Enums;
using Core.Models.Messaging.Email.Errors;
using Core.Models.Messaging.Email.Identifiers;
using Core.Models.Messaging.Email.Repositories;
using Core.Models.Messaging.Enums;
using Core.Models.Messaging.Sms;
using Core.Models.Messaging.Sms.Errors;
using Core.Models.Messaging.Sms.Identifiers;
using Core.Models.Messaging.Sms.Repositories;
using Core.Shared;
using Serilog;

internal sealed class GetMessageDetailsQueryHandler
: IQueryHandler<GetMessageDetailsQuery, MessageDetailResponse>
{
    private readonly IEmailRepository _emailRepository;
    private readonly ISmsRepository _smsRepository;
    private readonly ILogger _logger;

    public GetMessageDetailsQueryHandler(
        IEmailRepository emailRepository,
        ISmsRepository smsRepository,
        ILogger logger)
    {
        _emailRepository = emailRepository;
        _smsRepository = smsRepository;
        _logger = logger
            .ForContext<GetMessageDetailsQuery>();
    }

    public async Task<Result<MessageDetailResponse>> Handle(GetMessageDetailsQuery request, CancellationToken cancellationToken)
    {
        Result<MessageDetailResponse> message = request.MessageId switch
        {
            EmailId emailId => await BuildEmail(emailId, cancellationToken),
            SmsId smsId => await BuildSms(smsId, cancellationToken),
            _ => throw new InvalidCastException()
        };

        if (message.IsFailure)
        {
            return Result.Failure<MessageDetailResponse>(message.Error);
        }

        return message;
    }

    private async Task<Result<MessageDetailResponse>> BuildEmail(EmailId id, CancellationToken cancellationToken)
    {
        EmailMessage? message = await _emailRepository.GetById(id, cancellationToken);

        if (message is null)
        {
            _logger
                .ForContext(nameof(EmailId), id)
                .ForContext(nameof(Error), EmailMessagingErrors.NotFound(id))
                .Warning("Failed to retrieve message");

            return Result.Failure<MessageDetailResponse>(EmailMessagingErrors.NotFound(id));
        }

        MessageDetailResponse.Sender sender = new(message.From.Name, message.From.Destination);

        List<MessageDetailResponse.Recipient> recipients = [];

        foreach (EmailMessageRecipient recipient in message.Recipients)
        {
            recipients.Add(new(
                recipient.RecipientType,
                recipient.Name,
                recipient.Email));
        }

        MessageDetailResponse response = new(
            message.Id,
            MessageType.Email,
            sender,
            recipients,
            message.Subject,
            message.BodyHtml,
            message.Status,
            message.SentAt ?? DateTimeOffset.MinValue);

        return response;
    }

    private async Task<Result<MessageDetailResponse>> BuildSms(SmsId id, CancellationToken cancellationToken)
    {
        SmsMessage? message = await _smsRepository.GetById(id, cancellationToken);

        if (message is null)
        {
            _logger
                .ForContext(nameof(SmsId), id)
                .ForContext(nameof(Error), SmsMessagingErrors.NotFound(id), true)
                .Warning("Failed to retrieve message");

            return Result.Failure<MessageDetailResponse>(SmsMessagingErrors.NotFound(id));
        }

        MessageDetailResponse.Sender sender = new(message.Sender.Name, message.Sender.Number);

        List<MessageDetailResponse.Recipient> recipients = [
            new (EmailRecipientType.To, message.Recipient.Name, message.Recipient.Number)
        ];

        MessageDetailResponse response = new(
            message.Id,
            MessageType.Email,
            sender,
            recipients,
            message.Message,
            string.Empty,
            message.Status,
            message.CreatedAt);

        return response;
    }
}
