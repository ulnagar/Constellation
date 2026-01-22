namespace Constellation.Application.Domains.SciencePracs.Commands.CreateLesson;

using Abstractions.Messaging;
using Core.Abstractions.Repositories;
using Core.Models.Offerings;
using Core.Models.Offerings.Errors;
using Core.Models.Offerings.Repositories;
using Core.Models.SciencePracs;
using Core.Models.SciencePracs.Errors;
using Core.Models.Subjects;
using Core.Models.Subjects.Errors;
using Core.Models.Subjects.Identifiers;
using Core.Models.Subjects.Repositories;
using Core.Shared;
using Interfaces.Repositories;
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
        List<Course> courses = [];
        List<Offering> offerings = [];
        
        foreach (CourseId courseId in request.CourseIds)
        {
            Course? course = await _courseRepository.GetById(courseId, cancellationToken);

            if (course is null)
                continue;

            courses.Add(course);

            List<Offering> courseOfferings = await _offeringRepository.GetByCourseId(courseId, cancellationToken);

            offerings.AddRange(courseOfferings);
        }

        if (courses.Count == 0)
        {
            _logger
                .ForContext(nameof(CreateLessonCommand), request, true)
                .ForContext(nameof(Error), CourseErrors.NoneFound, true)
                .Warning("Failed to create Science Prac Lesson");

            return Result.Failure(CourseErrors.NoneFound);
        }

        if (courses.Select(course => course.Grade).Distinct().Count() > 1)
        {
            _logger
                .ForContext(nameof(CreateLessonCommand), request, true)
                .ForContext(nameof(Error), SciencePracLessonErrors.MustBeSameGrade, true)
                .Warning("Failed to create Science Prac Lesson");

            return Result.Failure(SciencePracLessonErrors.MustBeSameGrade);
        }

        offerings = offerings.Where(offering => offering.IsCurrent).ToList();

        if (offerings.Count == 0)
        {
            _logger
                .ForContext(nameof(CreateLessonCommand), request, true)
                .ForContext(nameof(Error), OfferingErrors.NoneFound, true)
                .Warning("Failed to create Science Prac Lesson");

            return Result.Failure(OfferingErrors.NoneFound);
        }

        Result<SciencePracLesson> lesson = SciencePracLesson.Create(
            request.Name,
            request.DueDate,
            courses.First().Grade,
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
