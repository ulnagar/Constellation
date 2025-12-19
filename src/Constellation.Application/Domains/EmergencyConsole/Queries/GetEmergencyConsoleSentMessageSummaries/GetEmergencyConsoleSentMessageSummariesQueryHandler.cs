namespace Constellation.Application.Domains.EmergencyConsole.Queries.GetEmergencyConsoleSentMessageSummaries;

using Abstractions.Messaging;
using Core.Models.EmergencyConsole;
using Core.Models.EmergencyConsole.Repositories;
using Core.Shared;
using Serilog;
using System.Collections.Generic;
using System.Threading.Tasks;

internal sealed class GetEmergencyConsoleSentMessageSummariesQueryHandler
: IQueryHandler<GetEmergencyConsoleSentMessageSummariesQuery, List<SentMessageSummary>>
{
    private readonly ISentMessageRepository _sentMessageRepository;
    private readonly ILogger _logger;

    public GetEmergencyConsoleSentMessageSummariesQueryHandler(
        ISentMessageRepository sentMessageRepository,
        ILogger logger)
    {
        _sentMessageRepository = sentMessageRepository;
        _logger = logger;
    }

    public async Task<Result<List<SentMessageSummary>>> Handle(GetEmergencyConsoleSentMessageSummariesQuery request, CancellationToken cancellationToken)
    {
        List<SentMessageSummary> summaries = [];

        List<SentMessage> messages = await _sentMessageRepository.GetMessageSummaries(cancellationToken);

        foreach (SentMessage message in messages)
            summaries.Add(new(message.Id, message.Message, message.Statuses.Count));

        return summaries;
    }
}
