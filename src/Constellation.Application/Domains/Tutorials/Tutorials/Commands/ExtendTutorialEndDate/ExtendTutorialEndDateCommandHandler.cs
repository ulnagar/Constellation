namespace Constellation.Application.Domains.Tutorials.Tutorials.Commands.ExtendTutorialEndDate;

using Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Services;
using Constellation.Core.Models.Tutorials;
using Constellation.Core.Models.Tutorials.Errors;
using Constellation.Core.Models.Tutorials.Repositories;
using Core.Abstractions.Clock;
using Core.Shared;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class ExtendTutorialEndDateCommandHandler
: ICommandHandler<ExtendTutorialEndDateCommand>
{
    private readonly ITutorialRepository _tutorialRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTime;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public ExtendTutorialEndDateCommandHandler(
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
            .ForContext<ExtendTutorialEndDateCommand>();
    }

    public async Task<Result> Handle(ExtendTutorialEndDateCommand request, CancellationToken cancellationToken)
    {
        Tutorial tutorial = await _tutorialRepository.GetById(request.TutorialId, cancellationToken);

        if (tutorial is null)
        {
            _logger
                .ForContext(nameof(ExtendTutorialEndDateCommand), request, true)
                .ForContext(nameof(Error), TutorialErrors.NotFound(request.TutorialId), true)
                .Warning("Failed to extend Tutorial by user {User}", _currentUserService.UserName);

            return Result.Failure(TutorialErrors.NotFound(request.TutorialId));
        }

        Result update = tutorial.Extend(request.EndDate, _dateTime);

        if (update.IsFailure)
        {
            _logger
                .ForContext(nameof(ExtendTutorialEndDateCommand), request, true)
                .ForContext(nameof(Tutorial), tutorial, true)
                .ForContext(nameof(Error), update.Error, true)
                .Warning("Failed to extend Tutorial by user {User}", _currentUserService.UserName);

            return Result.Failure(update.Error);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
