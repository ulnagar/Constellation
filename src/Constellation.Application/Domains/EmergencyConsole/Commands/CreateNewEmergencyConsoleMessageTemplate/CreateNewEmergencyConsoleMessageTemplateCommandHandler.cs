namespace Constellation.Application.Domains.EmergencyConsole.Commands.CreateNewEmergencyConsoleMessageTemplate;

using Abstractions.Messaging;
using Core.Abstractions.Services;
using Core.Models.Messaging.EmergencyConsole;
using Core.Models.Messaging.EmergencyConsole.Errors;
using Core.Models.Messaging.EmergencyConsole.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CreateNewEmergencyConsoleMessageTemplateCommandHandler
: ICommandHandler<CreateNewEmergencyConsoleMessageTemplateCommand>
{
    private readonly IMessageTemplateRepository _templateRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public CreateNewEmergencyConsoleMessageTemplateCommandHandler(
        IMessageTemplateRepository templateRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _templateRepository = templateRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<CreateNewEmergencyConsoleMessageTemplateCommand>();
    }

    public async Task<Result> Handle(CreateNewEmergencyConsoleMessageTemplateCommand request, CancellationToken cancellationToken)
    {
        MessageTemplate? existingTemplate = await _templateRepository.GetByName(request.Name, cancellationToken);

        if (existingTemplate is not null)
        {
            _logger
                .ForContext(nameof(CreateNewEmergencyConsoleMessageTemplateCommand), request, true)
                .ForContext(nameof(Error), MessageTemplateErrors.NameInUse(request.Name), true)
                .Warning("Failed to create new Emergency Console Message Template by user {User}", _currentUserService.UserName);

            return Result.Failure(MessageTemplateErrors.NameInUse(request.Name));
        }

        Result<MessageTemplate> template = MessageTemplate.Create(request.Type, request.Name, request.Template);

        if (template.IsFailure)
        {
            _logger
                .ForContext(nameof(CreateNewEmergencyConsoleMessageTemplateCommand), request, true)
                .ForContext(nameof(Error), template.Error, true)
                .Warning("Failed to create new Emergency Console Message Template by user {User}", _currentUserService.UserName);

            return Result.Failure(template.Error);
        }

        _templateRepository.Insert(template.Value);
        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
