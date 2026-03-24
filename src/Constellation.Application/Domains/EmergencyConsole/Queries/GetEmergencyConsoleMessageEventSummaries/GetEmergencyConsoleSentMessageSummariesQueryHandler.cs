namespace Constellation.Application.Domains.EmergencyConsole.Queries.GetEmergencyConsoleMessageEventSummaries;

using Abstractions.Messaging;
using Core.Models.Messaging.EmergencyConsole;
using Core.Models.Messaging.EmergencyConsole.Repositories;
using Core.Shared;
using Serilog;
using System.Collections.Generic;
using System.Threading.Tasks;

internal sealed class GetEmergencyConsoleMessageEventSummariesQueryHandler
: IQueryHandler<GetEmergencyConsoleMessageEventSummariesQuery, List<MessageEventSummary>>
{
    private readonly IMessageEventRepository _messageEventRepository;
    private readonly ILogger _logger;

    public GetEmergencyConsoleMessageEventSummariesQueryHandler(
        IMessageEventRepository messageEventRepository,
        ILogger logger)
    {
        _messageEventRepository = messageEventRepository;
        _logger = logger;
    }

    public async Task<Result<List<MessageEventSummary>>> Handle(GetEmergencyConsoleMessageEventSummariesQuery request, CancellationToken cancellationToken)
    {
        List<MessageEventSummary> summaries = [];

        List<MessageEvent> messages = await _messageEventRepository.GetEventSummaries(cancellationToken);

        foreach (MessageEvent message in messages)
            summaries.Add(new(
                message.Id, 
                message.Message, 
                message.SentAt, 
                message.SentBy, 
                message.Recipients.Count));

        return summaries;
    }
}
