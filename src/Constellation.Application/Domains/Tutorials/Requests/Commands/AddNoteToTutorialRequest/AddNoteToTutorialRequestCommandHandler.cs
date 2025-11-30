namespace Constellation.Application.Domains.Tutorials.Requests.Commands.AddNoteToTutorialRequest;

using Abstractions.Messaging;
using Constellation.Core.Abstractions.Services;
using Constellation.Core.Models.Tutorials.Errors;
using Core.Abstractions.Clock;
using Core.Models.Tutorials;
using Core.Models.Tutorials.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class AddNoteToTutorialRequestCommandHandler
: ICommandHandler<AddNoteToTutorialRequestCommand>
{
    private readonly ITutorialRepository _tutorialRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTime;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public AddNoteToTutorialRequestCommandHandler(
        ITutorialRepository tutorialRepository,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTime,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _tutorialRepository = tutorialRepository;
        _currentUserService = currentUserService;
        _dateTime = dateTime;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<AddNoteToTutorialRequestCommand>();
    }

    public async Task<Result> Handle(AddNoteToTutorialRequestCommand request, CancellationToken cancellationToken)
    {
        Request tutorialRequest = await _tutorialRepository.GetRequestById(request.RequestId, cancellationToken);

        if (tutorialRequest is null)
        {
            _logger
                .ForContext(nameof(AddNoteToTutorialRequestCommand), request, true)
                .ForContext(nameof(Error), TutorialRequestErrors.NotFound(request.RequestId), true)
                .Warning("Failed to add note to Tutorial Request");

            return Result.Failure(TutorialRequestErrors.NotFound(request.RequestId));
        }

        tutorialRequest.AddNote(request.Message, _currentUserService.UserName, _dateTime);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
