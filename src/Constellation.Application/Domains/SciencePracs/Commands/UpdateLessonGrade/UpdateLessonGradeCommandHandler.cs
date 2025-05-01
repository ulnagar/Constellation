namespace Constellation.Application.Domains.SciencePracs.Commands.UpdateLessonGrade;

using Abstractions.Messaging;
using Core.Abstractions.Repositories;
using Core.Models.Offerings.Identifiers;
using Core.Models.SciencePracs;
using Core.Models.SciencePracs.Errors;
using Core.Models.Subjects;
using Core.Models.Subjects.Errors;
using Core.Models.Subjects.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class UpdateLessonGradeCommandHandler
: ICommandHandler<UpdateLessonGradeCommand>
{
    private readonly ILessonRepository _lessonRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public UpdateLessonGradeCommandHandler(
        ILessonRepository lessonRepository,
        ICourseRepository courseRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _lessonRepository = lessonRepository;
        _courseRepository = courseRepository;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<UpdateLessonGradeCommand>();
    }

    public async Task<Result> Handle(UpdateLessonGradeCommand request, CancellationToken cancellationToken)
    {
        SciencePracLesson lesson = await _lessonRepository.GetById(request.LessonId, cancellationToken);

        if (lesson is null)
        {
            _logger
                .ForContext(nameof(UpdateLessonGradeCommand), request, true)
                .ForContext(nameof(Error), SciencePracLessonErrors.NotFound(request.LessonId), true)
                .Warning("Failed to update Science Prac Lesson grade");

            return Result.Failure(SciencePracLessonErrors.NotFound(request.LessonId));
        }

        OfferingId? offeringId = lesson.Offerings.FirstOrDefault()?.OfferingId;

        if (offeringId is null)
        {
            _logger
                .ForContext(nameof(UpdateLessonGradeCommand), request, true)
                .ForContext(nameof(SciencePracLesson), lesson, true)
                .ForContext(nameof(Error), SciencePracLessonErrors.NoOfferingsLinked, true)
                .Warning("Failed to update Science Prac Lesson grade");

            return Result.Failure(SciencePracLessonErrors.NoOfferingsLinked);
        }

        Course course = await _courseRepository.GetByOfferingId(offeringId.Value, cancellationToken);

        if (course is null)
        {
            _logger
                .ForContext(nameof(UpdateLessonGradeCommand), request, true)
                .ForContext(nameof(SciencePracLesson), lesson, true)
                .ForContext(nameof(Error), CourseErrors.NotFoundByOfferingId(offeringId.Value), true)
                .Warning("Failed to update Science Prac Lesson grade");

            return Result.Failure(CourseErrors.NotFoundByOfferingId(offeringId.Value));
        }

        lesson.UpdateGrade(course.Grade);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}