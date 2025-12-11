namespace Constellation.Application.Domains.EmergencyConsole.Commands.DeleteEmergencyConsoleMessageTemplate;

using Abstractions.Messaging;
using Constellation.Core.Models.EmergencyConsole.Errors;
using Core.Abstractions.Services;
using Core.Models.EmergencyConsole;
using Core.Models.EmergencyConsole.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class DeleteEmergencyConsoleMessageTemplateCommandHandler
: ICommandHandler<DeleteEmergencyConsoleMessageTemplateCommand>
{
    private readonly IMessageTemplateRepository _templateRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public DeleteEmergencyConsoleMessageTemplateCommandHandler(
        IMessageTemplateRepository templateRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _templateRepository = templateRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<DeleteEmergencyConsoleMessageTemplateCommand>();
    }

    public async Task<Result> Handle(DeleteEmergencyConsoleMessageTemplateCommand request, CancellationToken cancellationToken)
    {
        MessageTemplate? existingTemplate = await _templateRepository.GetById(request.Id, cancellationToken);

        if (existingTemplate is null)
        {
            _logger
                .ForContext(nameof(DeleteEmergencyConsoleMessageTemplateCommand), request, true)
                .ForContext(nameof(Error), MessageTemplateErrors.NotFound(request.Id), true)
                .Warning("Failed to delete Emergency Console Message Template with Id '{Id}' by user {User}", request.Id, _currentUserService.UserName);

            return Result.Failure(MessageTemplateErrors.NotFound(request.Id));
        }

        _templateRepository.Remove(existingTemplate);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
