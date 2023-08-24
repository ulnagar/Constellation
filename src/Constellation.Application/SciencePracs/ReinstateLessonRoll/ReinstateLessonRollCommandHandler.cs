namespace Constellation.Application.SciencePracs.ReinstateLessonRoll;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Errors;
using Constellation.Core.Models.SciencePracs;
using Constellation.Core.Shared;
using Serilog;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class ReinstateLessonRollCommandHandler
    : ICommandHandler<ReinstateLessonRollCommand>
{
    private readonly ILessonRepository _lessonRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public ReinstateLessonRollCommandHandler(
        ILessonRepository lessonRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _lessonRepository = lessonRepository;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<ReinstateLessonRollCommand>();
    }

    public async Task<Result> Handle(ReinstateLessonRollCommand request, CancellationToken cancellationToken)
    {
        SciencePracLesson lesson = await _lessonRepository.GetById(request.LessonId, cancellationToken);

        if (lesson is null)
        {
            _logger.Warning("Could not find Science Prac Lesson with Id {id}", request.LessonId);

            return Result.Failure(DomainErrors.SciencePracs.Lesson.NotFound(request.LessonId));
        }

        SciencePracRoll roll = lesson.Rolls.SingleOrDefault(roll => roll.Id == request.RollId);

        if (roll is null)
        {
            _logger.Warning("Could not find Science Prac Roll with Id {id}", request.RollId);

            return Result.Failure(DomainErrors.SciencePracs.Roll.NotFound(request.RollId));
        }

        if (roll.Status != Core.Enums.LessonStatus.Cancelled)
        {
            _logger.Warning("Cannot reinstate a roll that has not already been submitted");

            return Result.Failure(DomainErrors.SciencePracs.Roll.CannotReinstateRoll);
        }

        Result reinstateRequest = roll.ReinstateRoll();

        if (reinstateRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(reinstateRequest.Error), reinstateRequest.Error)
                .Warning("Cannot reinstate roll");

            return Result.Failure(reinstateRequest.Error);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
