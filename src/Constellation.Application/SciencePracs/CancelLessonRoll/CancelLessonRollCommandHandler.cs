namespace Constellation.Application.SciencePracs.CancelLessonRoll;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Errors;
using Constellation.Core.Models.SciencePracs;
using Constellation.Core.Shared;
using Core.Abstractions.Clock;
using Core.Abstractions.Services;
using Core.Models.SciencePracs.Errors;
using Serilog;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CancelLessonRollCommandHandler
    : ICommandHandler<CancelLessonRollCommand>
{
    private readonly ILessonRepository _lessonRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger _logger;

    public CancelLessonRollCommandHandler(
        ILessonRepository lessonRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTime,
        ILogger logger)
    {
        _lessonRepository = lessonRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _dateTime = dateTime;
        _logger = logger.ForContext<CancelLessonRollCommand>();
    }

    public async Task<Result> Handle(CancelLessonRollCommand request, CancellationToken cancellationToken)
    {
        SciencePracLesson lesson = await _lessonRepository.GetById(request.LessonId, cancellationToken);

        if (lesson is null)
        {
            _logger.Warning("Could not find Science Prac Lesson with Id {id}", request.LessonId);

            return Result.Failure(SciencePracLessonErrors.NotFound(request.LessonId));
        }

        SciencePracRoll roll = lesson.Rolls.SingleOrDefault(roll => roll.Id == request.RollId);

        if (roll is null)
        {
            _logger.Warning("Could not find Science Prac Roll with Id {id}", request.RollId);

            return Result.Failure(SciencePracRollErrors.NotFound(request.RollId));
        }

        if (roll.Status != Core.Enums.LessonStatus.Active)
        {
            _logger.Warning("Cannot cancel a roll that has already been submitted");

            return Result.Failure(SciencePracRollErrors.CannotCancelCompletedRoll);
        }

        Result cancelRequest = roll.CancelRoll($"{request.Comment} Roll cancelled by {_currentUserService.UserName} at {_dateTime.Now}");

        if (cancelRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(cancelRequest.Error), cancelRequest.Error)
                .Warning("Cannot cancel roll");

            return Result.Failure(cancelRequest.Error);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
