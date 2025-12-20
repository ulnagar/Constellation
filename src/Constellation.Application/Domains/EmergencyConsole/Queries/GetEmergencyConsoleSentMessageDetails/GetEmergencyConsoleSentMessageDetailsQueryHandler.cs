namespace Constellation.Application.Domains.EmergencyConsole.Queries.GetEmergencyConsoleSentMessageDetails;

using Abstractions.Messaging;
using Core.Models.EmergencyConsole;
using Core.Models.EmergencyConsole.Errors;
using Core.Models.EmergencyConsole.Repositories;
using Core.Shared;
using Serilog;
using System.Threading.Tasks;

internal sealed class GetEmergencyConsoleSentMessageDetailsQueryHandler
: IQueryHandler<GetEmergencyConsoleSentMessageDetailsQuery, SentMessageDetail>
{
    private readonly ISentMessageRepository _sentMessageRepository;
    private readonly ILogger _logger;

    public GetEmergencyConsoleSentMessageDetailsQueryHandler(
        ISentMessageRepository sentMessageRepository,
        ILogger logger)
    {
        _sentMessageRepository = sentMessageRepository;
        _logger = logger
            .ForContext<GetEmergencyConsoleSentMessageDetailsQuery>();
    }

    public async Task<Result<SentMessageDetail>> Handle(GetEmergencyConsoleSentMessageDetailsQuery request, CancellationToken cancellationToken)
    {
        SentMessage? message = await _sentMessageRepository.GetMessageById(request.EventId, cancellationToken);

        if (message is null)
        {
            _logger
                .ForContext(nameof(GetEmergencyConsoleSentMessageDetailsQuery), request, true)
                .ForContext(nameof(Error), SentMessageErrors.NotFound(request.EventId), true)
                .Warning("Failed to retrieve details of Emergency Message");

            return Result.Failure<SentMessageDetail>(SentMessageErrors.NotFound(request.EventId));
        }

        List<SentMessageDetail.RecipientStatus> recipients = [];

        foreach (var status in message.Statuses)
        {
            recipients.Add(new SentMessageDetail.RecipientStatus(
                status.Type,
                status.RecipientAddress,
                status.RecipientName,
                status.Sent));
        }

        SentMessageDetail detail = new(
            message.Id,
            message.SentAt,
            message.SentBy,
            message.Message,
            recipients);

        return detail;
    }
}