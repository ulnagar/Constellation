namespace Constellation.Application.SciencePracs.UpdateLesson;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions;
using Constellation.Core.Errors;
using Constellation.Core.Models.SciencePracs;
using Constellation.Core.Shared;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;

internal sealed class UpdateLessonCommandHandler
    : ICommandHandler<UpdateLessonCommand>
{
    private readonly ILessonRepository _lessonRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public UpdateLessonCommandHandler(
        ILessonRepository lessonRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _lessonRepository = lessonRepository;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<UpdateLessonCommand>();
    }

    public async Task<Result> Handle(UpdateLessonCommand request, CancellationToken cancellationToken)
    {
        SciencePracLesson lesson = await _lessonRepository.GetById(request.LessonId, cancellationToken);

        if (lesson is null)
        {
            _logger.Warning("Could not find a Science Prac Lesson with the Id {id}", request.LessonId);

            return Result.Failure(DomainErrors.SciencePracs.Lesson.NotFound(request.LessonId));
        }

        if (request.DueDate < DateOnly.FromDateTime(DateTime.Today))
        {
            _logger.Warning("Cannot set due date for Science Prac Lesson in the past");

            return Result.Failure(DomainErrors.SciencePracs.Lesson.PastDueDate(request.DueDate));
        }

        Result updateRequest = lesson.Update(request.Name, request.DueDate);

        if (updateRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(UpdateLessonCommand), request, true)
                .ForContext(nameof(updateRequest.Error), updateRequest.Error, true)
                .Warning("Could not update Science Prac Lesson");

            return Result.Failure(updateRequest.Error);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
