namespace Constellation.Application.Domains.Messaging.EmergencyConsole.Queries.GetAllEmergencyConsoleMessageTemplates;

using Abstractions.Messaging;
using Core.Models.Messaging.EmergencyConsole;
using Core.Models.Messaging.EmergencyConsole.Repositories;
using Core.Shared;
using Serilog;

internal sealed class GetAllEmergencyConsoleMessageTemplatesQueryHandler
: IQueryHandler<GetAllEmergencyConsoleMessageTemplatesQuery, List<MessageTemplate>>
{
    private readonly IMessageTemplateRepository _templateRepository;
    private readonly ILogger _logger;

    public GetAllEmergencyConsoleMessageTemplatesQueryHandler(
        IMessageTemplateRepository templateRepository,
        ILogger logger)
    {
        _templateRepository = templateRepository;
        _logger = logger
            .ForContext<GetAllEmergencyConsoleMessageTemplatesQuery>();
    }

    public async Task<Result<List<MessageTemplate>>> Handle(GetAllEmergencyConsoleMessageTemplatesQuery request, CancellationToken cancellationToken)
    {
        List<MessageTemplate> templates = await _templateRepository.GetAll(cancellationToken);

        return templates;
    }
}
