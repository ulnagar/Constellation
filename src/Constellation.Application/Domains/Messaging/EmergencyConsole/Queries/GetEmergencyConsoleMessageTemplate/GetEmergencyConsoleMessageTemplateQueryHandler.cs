namespace Constellation.Application.Domains.Messaging.EmergencyConsole.Queries.GetEmergencyConsoleMessageTemplate;

using Abstractions.Messaging;
using Core.Abstractions.Services;
using Core.Models.Messaging.EmergencyConsole;
using Core.Models.Messaging.EmergencyConsole.Errors;
using Core.Models.Messaging.EmergencyConsole.Repositories;
using Core.Shared;
using Serilog;

internal sealed class GetEmergencyConsoleMessageTemplateQueryHandler
: IQueryHandler<GetEmergencyConsoleMessageTemplateQuery, MessageTemplate>
{
    private readonly IMessageTemplateRepository _templateRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public GetEmergencyConsoleMessageTemplateQueryHandler(
        IMessageTemplateRepository templateRepository,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _templateRepository = templateRepository;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<GetEmergencyConsoleMessageTemplateQuery>();
    }

    public async Task<Result<MessageTemplate>> Handle(GetEmergencyConsoleMessageTemplateQuery request, CancellationToken cancellationToken)
    {
        MessageTemplate template = await _templateRepository.GetById(request.Id, cancellationToken);

        if (template is null)
        {
            _logger
                .ForContext(nameof(GetEmergencyConsoleMessageTemplateQuery), request, true)
                .ForContext(nameof(Error), MessageTemplateErrors.NotFound(request.Id), true)
                .Warning("Failed to retrieve Message Template with Id {Id} by user {User}", request.Id, _currentUserService.UserName);

            return Result.Failure<MessageTemplate>(MessageTemplateErrors.NotFound(request.Id));
        }

        return template;
    }
}
