namespace Constellation.Application.SciencePracs.CreateLesson;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Extensions;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions;
using Constellation.Core.Errors;
using Constellation.Core.Models.SciencePracs;
using Constellation.Core.Models.Subjects;
using Constellation.Core.Shared;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CreateLessonCommandHandler
    : ICommandHandler<CreateLessonCommand>
{
    private readonly ILessonRepository _lessonRepository;
    private readonly ICourseOfferingRepository _courseOfferingRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public CreateLessonCommandHandler(
        ILessonRepository lessonRepository,
        ICourseOfferingRepository courseOfferingRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _lessonRepository = lessonRepository;
        _courseOfferingRepository = courseOfferingRepository;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<CreateLessonCommand>();
    }

    public async Task<Result> Handle(CreateLessonCommand request, CancellationToken cancellationToken)
    {
        List<Offering> offerings = await _courseOfferingRepository.GetByCourseId(request.CourseId, cancellationToken);

        offerings = offerings.Where(offering => offering.IsCurrent()).ToList();

        if (offerings.Count == 0)
        {
            _logger.Warning("Could not find any offerings for course with Id {id}", request.CourseId);

            return Result.Failure(DomainErrors.Subjects.Course.NoOfferings(request.CourseId));
        }

        Result<SciencePracLesson> lesson = SciencePracLesson.Create(
            request.Name,
            request.DueDate,
            offerings.Select(offering => offering.Id).ToList(),
            request.DoNotGenerateRolls);

        if (lesson.IsFailure)
        {
            _logger
                .ForContext(nameof(lesson.Error), lesson.Error, true)
                .Warning("Could not create Science Prac Lesson");

            return Result.Failure(lesson.Error);
        }

        _lessonRepository.Insert(lesson.Value);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
