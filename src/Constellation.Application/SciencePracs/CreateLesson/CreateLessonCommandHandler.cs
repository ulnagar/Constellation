namespace Constellation.Application.SciencePracs.CreateLesson;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Models.SciencePracs;
using Constellation.Core.Models.Subjects.Errors;
using Constellation.Core.Shared;
using Core.Models.Subjects;
using Core.Models.Subjects.Repositories;
using Courses.Models;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CreateLessonCommandHandler
    : ICommandHandler<CreateLessonCommand>
{
    private readonly ILessonRepository _lessonRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public CreateLessonCommandHandler(
        ILessonRepository lessonRepository,
        IOfferingRepository offeringRepository,
        ICourseRepository courseRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _lessonRepository = lessonRepository;
        _offeringRepository = offeringRepository;
        _courseRepository = courseRepository;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<CreateLessonCommand>();
    }

    public async Task<Result> Handle(CreateLessonCommand request, CancellationToken cancellationToken)
    {
        Course course = await _courseRepository.GetById(request.CourseId, cancellationToken);

        if (course is null)
        {
            _logger
                .ForContext(nameof(CreateLessonCommand), request, true)
                .ForContext(nameof(Error), CourseErrors.NotFound(request.CourseId), true)
                .Warning("Failed to create Science Prac Lesson");

            return Result.Failure(CourseErrors.NotFound(request.CourseId));
        }

        List<Offering> offerings = await _offeringRepository.GetByCourseId(request.CourseId, cancellationToken);

        offerings = offerings.Where(offering => offering.IsCurrent).ToList();

        if (offerings.Count == 0)
        {
            _logger
                .ForContext(nameof(CreateLessonCommand), request, true)
                .ForContext(nameof(Error), CourseErrors.NoOfferings(request.CourseId), true)
                .Warning("Failed to create Science Prac Lesson");

            return Result.Failure(CourseErrors.NoOfferings(request.CourseId));
        }

        Result<SciencePracLesson> lesson = SciencePracLesson.Create(
            request.Name,
            request.DueDate,
            course.Grade,
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
