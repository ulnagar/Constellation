namespace Constellation.Application.Domains.Messaging.EmergencyConsole.Queries.GetEmergencyConsoleMessageEventDetails;

using Abstractions.Messaging;
using Core.Models.Messaging.EmergencyConsole;
using Core.Models.Messaging.EmergencyConsole.Errors;
using Core.Models.Messaging.EmergencyConsole.Repositories;
using Core.Shared;
using Serilog;

internal sealed class GetEmergencyConsoleMessageEventDetailsQueryHandler
: IQueryHandler<GetEmergencyConsoleMessageEventDetailsQuery, MessageEventDetail>
{
    private readonly IMessageEventRepository _messageEventRepository;
    private readonly ILogger _logger;

    public GetEmergencyConsoleMessageEventDetailsQueryHandler(
        IMessageEventRepository messageEventRepository,
        ILogger logger)
    {
        _messageEventRepository = messageEventRepository;
        _logger = logger
            .ForContext<GetEmergencyConsoleMessageEventDetailsQuery>();
    }

    public async Task<Result<MessageEventDetail>> Handle(GetEmergencyConsoleMessageEventDetailsQuery request, CancellationToken cancellationToken)
    {
        MessageEvent? message = await _messageEventRepository.GetEventById(request.EventId, cancellationToken);

        if (message is null)
        {
            _logger
                .ForContext(nameof(GetEmergencyConsoleMessageEventDetailsQuery), request, true)
                .ForContext(nameof(Error), MessageEventErrors.NotFound(request.EventId), true)
                .Warning("Failed to retrieve details of Emergency MessageEvent");

            return Result.Failure<MessageEventDetail>(MessageEventErrors.NotFound(request.EventId));
        }

        List<MessageEventDetail.RecipientStatus> recipients = [];

        foreach (var status in message.Recipients)
        {
            recipients.Add(new MessageEventDetail.RecipientStatus(
                status.Type,
                status.RecipientAddress,
                status.RecipientName,
                status.Status));
        }

        MessageEventDetail eventDetail = new(
            message.Id,
            message.SentAt,
            message.SentBy,
            message.Message,
            recipients);

        return eventDetail;
    }
}