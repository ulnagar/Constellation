namespace Constellation.Application.Domains.Tutorials.Commands.AddMultipleSessionsToTutorial;

using Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models.Timetables;
using Constellation.Core.Models.Timetables.Identifiers;
using Constellation.Core.Models.Timetables.Repositories;
using Constellation.Core.Models.Tutorials;
using Core.Models.Timetables.Errors;
using Core.Models.Tutorials.Errors;
using Core.Models.Tutorials.Repositories;
using Core.Shared;
using Serilog;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class AddMultipleSessionsToTutorialCommandHandler
    : ICommandHandler<AddMultipleSessionsToTutorialCommand>
{
    private readonly ITutorialRepository _tutorialRepository;
    private readonly IPeriodRepository _periodRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public AddMultipleSessionsToTutorialCommandHandler(
        ITutorialRepository tutorialRepository,
        IPeriodRepository periodRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _tutorialRepository = tutorialRepository;
        _periodRepository = periodRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(AddMultipleSessionsToTutorialCommand request, CancellationToken cancellationToken)
    {
        Tutorial tutorial = await _tutorialRepository.GetById(request.TutorialId, cancellationToken);

        if (tutorial is null)
        {
            _logger
                .ForContext(nameof(AddMultipleSessionsToTutorialCommand), request, true)
                .ForContext(nameof(Error), TutorialErrors.NotFound(request.TutorialId), true)
                .Warning("Failed to add multiple Sessions to Tutorial");

            return Result.Failure(TutorialErrors.NotFound(request.TutorialId));
        }

        foreach (PeriodId periodId in request.PeriodIds)
        {
            Period period = await _periodRepository.GetById(periodId, cancellationToken);

            if (period is null)
            {
                _logger
                    .ForContext(nameof(AddMultipleSessionsToTutorialCommand), request, true)
                    .ForContext(nameof(Error), PeriodErrors.NotFound(periodId), true)
                    .Warning("Failed to add multiple Sessions to Tutorial");

                return Result.Failure(PeriodErrors.NotFound(periodId));
            }

            if (tutorial.Sessions.Any(session => !session.IsDeleted && session.PeriodId == periodId))
            {
                // Existing non-deleted session for this period found

                continue;
            }

            tutorial.AddSession(periodId, request.StaffId);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
