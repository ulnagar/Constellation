namespace Constellation.Application.Domains.Tutorials.Tutorials.Commands.EndTutorial;

using Abstractions.Messaging;
using Constellation.Core.Abstractions.Services;
using Constellation.Core.Models.Tutorials;
using Constellation.Core.Models.Tutorials.Errors;
using Core.Abstractions.Clock;
using Core.Models.Tutorials.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class EndTutorialCommandHandler
: ICommandHandler<EndTutorialCommand>
{
    private readonly ITutorialRepository _tutorialRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTime;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public EndTutorialCommandHandler(
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
            .ForContext<EndTutorialCommand>();
    }

    public async Task<Result> Handle(EndTutorialCommand request, CancellationToken cancellationToken)
    {
        Tutorial tutorial = await _tutorialRepository.GetById(request.TutorialId, cancellationToken);

        if (tutorial is null)
        {
            _logger
                .ForContext(nameof(EndTutorialCommand), request, true)
                .ForContext(nameof(Error), TutorialErrors.NotFound(request.TutorialId), true)
                .Warning("Failed to end Tutorial by user {User}", _currentUserService.UserName);

            return Result.Failure(TutorialErrors.NotFound(request.TutorialId));
        }

        Result update = tutorial.Cancel(request.EndDate, _dateTime);

        if (update.IsFailure)
        {
            _logger
                .ForContext(nameof(EndTutorialCommand), request, true)
                .ForContext(nameof(Tutorial), tutorial, true)
                .ForContext(nameof(Error), update.Error, true)
                .Warning("Failed to end Tutorial by user {User}", _currentUserService.UserName);

            return Result.Failure(update.Error);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
