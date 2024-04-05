namespace Constellation.Application.WorkFlows.AddActionNote;

using Abstractions.Messaging;
using Constellation.Core.Models.WorkFlow.Errors;
using Core.Abstractions.Clock;
using Core.Abstractions.Services;
using Core.Models.WorkFlow;
using Core.Models.WorkFlow.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class AddActionNoteCommandHandler
: ICommandHandler<AddActionNoteCommand>
{
    private readonly ICaseRepository _caseRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTime;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public AddActionNoteCommandHandler(
        ICaseRepository caseRepository,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTime,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _caseRepository = caseRepository;
        _currentUserService = currentUserService;
        _dateTime = dateTime;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<AddActionNoteCommand>();
    }

    public async Task<Result> Handle(AddActionNoteCommand request, CancellationToken cancellationToken)
    {
        Case item = await _caseRepository.GetById(request.CaseId, cancellationToken);

        if (item is null)
        {
            _logger
                .ForContext(nameof(AddActionNoteCommand), request, true)
                .ForContext(nameof(Error), CaseErrors.Case.NotFound(request.CaseId), true)
                .Warning("Failed to add note to Action");

            return Result.Failure(CaseErrors.Case.NotFound(request.CaseId));
        }

        Action action = item.Actions.FirstOrDefault(action => action.Id == request.ActionId);

        if (action is null)
        {
            _logger
                .ForContext(nameof(AddActionNoteCommand), request, true)
                .ForContext(nameof(Case), item, true)
                .ForContext(nameof(Error), CaseErrors.Action.NotFound(request.ActionId), true)
                .Warning("Failed to add note to Action");

            return Result.Failure(CaseErrors.Action.NotFound(request.ActionId));
        }

        Result noteRequest = item.AddActionNote(action.Id, request.Note, _currentUserService.UserName);

        if (noteRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(AddActionNoteCommand), request, true)
                .ForContext(nameof(Case), item, true)
                .ForContext(nameof(Action), action, true)
                .ForContext(nameof(Error), noteRequest.Error, true)
                .Warning("Failed to add note to Action");

            return Result.Failure(noteRequest.Error);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
