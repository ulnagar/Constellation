namespace Constellation.Application.Domains.SciencePracs.Commands.CancelLesson;

using Abstractions.Messaging;
using Core.Abstractions.Repositories;
using Core.Models.SciencePracs;
using Core.Models.SciencePracs.Errors;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CancelLessonCommandHandler
    : ICommandHandler<CancelLessonCommand>
{
    private readonly ILessonRepository _lessonRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public CancelLessonCommandHandler(
        ILessonRepository lessonRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _lessonRepository = lessonRepository;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<CancelLessonCommand>();
    }

    public async Task<Result> Handle(CancelLessonCommand request, CancellationToken cancellationToken)
    {
        SciencePracLesson lesson = await _lessonRepository.GetById(request.LessonId, cancellationToken);

        if (lesson is null)
        {
            _logger.Warning("Could not find Science Prac Lesson with Id {id}", request.LessonId);

            return Result.Failure(SciencePracLessonErrors.NotFound(request.LessonId));
        }

        if (lesson.Rolls.Any(roll => roll.Status == Core.Enums.LessonStatus.Completed))
        {
            _logger
                .ForContext(nameof(CancelLessonCommand), request, true)
                .Warning("Cannot cancel lesson with completed rolls");

            return Result.Failure(SciencePracLessonErrors.RollCompleted);
        }

        _lessonRepository.Delete(lesson);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}