namespace Constellation.Application.Domains.EmergencyConsole.Queries.GetEmergencyConsoleMessageTemplates;

using Abstractions.Messaging;
using Core.Models.EmergencyConsole;
using Core.Models.EmergencyConsole.Repositories;
using Core.Shared;
using Serilog;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetEmergencyConsoleMessageTemplatesQueryHandler
: IQueryHandler<GetEmergencyConsoleMessageTemplatesQuery, List<MessageTemplate>>
{
    private readonly IMessageTemplateRepository _templateRepository;
    private readonly ILogger _logger;

    public GetEmergencyConsoleMessageTemplatesQueryHandler(
        IMessageTemplateRepository templateRepository,
        ILogger logger)
    {
        _templateRepository = templateRepository;
        _logger = logger
            .ForContext<GetEmergencyConsoleMessageTemplatesQuery>();
    }

    public async Task<Result<List<MessageTemplate>>> Handle(GetEmergencyConsoleMessageTemplatesQuery request, CancellationToken cancellationToken)
    {
        List<MessageTemplate> templates = await _templateRepository.GetAll(cancellationToken);

        return templates;
    }
}
