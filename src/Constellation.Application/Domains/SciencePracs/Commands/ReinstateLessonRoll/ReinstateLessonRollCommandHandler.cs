namespace Constellation.Application.Domains.SciencePracs.Commands.ReinstateLessonRoll;

using Abstractions.Messaging;
using Core.Abstractions.Repositories;
using Core.Abstractions.Services;
using Core.Enums;
using Core.Models.SciencePracs;
using Core.Models.SciencePracs.Errors;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class ReinstateLessonRollCommandHandler
    : ICommandHandler<ReinstateLessonRollCommand>
{
    private readonly ILessonRepository _lessonRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public ReinstateLessonRollCommandHandler(
        ILessonRepository lessonRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _lessonRepository = lessonRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger.ForContext<ReinstateLessonRollCommand>();
    }

    public async Task<Result> Handle(ReinstateLessonRollCommand request, CancellationToken cancellationToken)
    {
        SciencePracLesson? lesson = await _lessonRepository.GetById(request.LessonId, cancellationToken);

        if (lesson is null)
        {
            _logger
                .ForContext(nameof(ReinstateLessonRollCommand), request, true)
                .ForContext(nameof(Error), SciencePracLessonErrors.NotFound(request.LessonId), true)
                .Warning("Failed to reinstate Science Prac Roll by user {User}", _currentUserService.UserName);

            return Result.Failure(SciencePracLessonErrors.NotFound(request.LessonId));
        }

        SciencePracRoll? roll = lesson.Rolls.SingleOrDefault(roll => roll.Id == request.RollId);

        if (roll is null)
        {
            _logger
                .ForContext(nameof(ReinstateLessonRollCommand), request, true)
                .ForContext(nameof(Error), SciencePracRollErrors.NotFound(request.RollId), true)
                .Warning("Failed to reinstate Science Prac Roll by user {User}", _currentUserService.UserName);

            return Result.Failure(SciencePracRollErrors.NotFound(request.RollId));
        }

        if (roll.Status != LessonStatus.Cancelled && roll.Status != LessonStatus.Concern)
        {
            _logger
                .ForContext(nameof(ReinstateLessonRollCommand), request, true)
                .ForContext(nameof(Error), SciencePracRollErrors.NotFound(request.RollId), true)
                .Warning("Failed to reinstate Science Prac Roll by user {User}", _currentUserService.UserName);

            return Result.Failure(SciencePracRollErrors.CannotReinstateRoll);
        }

        Result reinstateRequest = roll.ReinstateRoll();

        if (reinstateRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(ReinstateLessonRollCommand), request, true)
                .ForContext(nameof(Error), reinstateRequest.Error, true)
                .Warning("Failed to reinstate Science Prac Roll by user {User}", _currentUserService.UserName);

            return Result.Failure(reinstateRequest.Error);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
