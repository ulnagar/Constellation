namespace Constellation.Application.SciencePracs.SubmitRoll;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Abstractions;
using Constellation.Core.Errors;
using Constellation.Core.Models.SciencePracs;
using Constellation.Core.Shared;
using Serilog;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class SubmitRollCommandHandler
    : ICommandHandler<SubmitRollCommand>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ILessonRepository _lessonRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public SubmitRollCommandHandler(
        ICurrentUserService currentUserService,
        ILessonRepository lessonRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _currentUserService = currentUserService;
        _lessonRepository = lessonRepository;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<SubmitRollCommand>();
    }

    public async Task<Result> Handle(SubmitRollCommand request, CancellationToken cancellationToken) 
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

        Result submitRequest = roll.MarkRoll(
            0,
            _currentUserService.UserName,
            request.LessonDate,
            request.Comment,
            request.PresentStudents,
            request.AbsentStudents);

        if (submitRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(submitRequest.Error), submitRequest.Error, true)
                .Warning("Could not mark roll due to error");

            return Result.Failure(submitRequest.Error);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
