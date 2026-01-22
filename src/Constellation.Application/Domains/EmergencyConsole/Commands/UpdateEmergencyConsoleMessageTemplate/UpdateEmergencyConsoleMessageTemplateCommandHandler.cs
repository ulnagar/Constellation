namespace Constellation.Application.Domains.EmergencyConsole.Commands.UpdateEmergencyConsoleMessageTemplate;

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

internal sealed class UpdateEmergencyConsoleMessageTemplateCommandHandler
: ICommandHandler<UpdateEmergencyConsoleMessageTemplateCommand>
{
    private readonly IMessageTemplateRepository _templateRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public UpdateEmergencyConsoleMessageTemplateCommandHandler(
        IMessageTemplateRepository templateRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _templateRepository = templateRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<UpdateEmergencyConsoleMessageTemplateCommand>();
    }

    public async Task<Result> Handle(UpdateEmergencyConsoleMessageTemplateCommand request, CancellationToken cancellationToken)
    {
        MessageTemplate? existingTemplate = await _templateRepository.GetById(request.Id, cancellationToken);

        if (existingTemplate is null)
        {
            _logger
                .ForContext(nameof(UpdateEmergencyConsoleMessageTemplateCommand), request, true)
                .ForContext(nameof(Error), MessageTemplateErrors.NotFound(request.Id), true)
                .Warning("Failed to update Emergency Console Message Template with Id '{Id}' by user {User}", request.Id, _currentUserService.UserName);

            return Result.Failure(MessageTemplateErrors.NotFound(request.Id));
        }

        Result update = existingTemplate.Update(request.Name, request.Template);

        if (update.IsFailure)
        {
            _logger
                .ForContext(nameof(UpdateEmergencyConsoleMessageTemplateCommand), request, true)
                .ForContext(nameof(Error), update.Error, true)
                .Warning("Failed to update Emergency Console Message Template with Id '{Id}' by user {User}", request.Id, _currentUserService.UserName);

            return Result.Failure(update.Error);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
