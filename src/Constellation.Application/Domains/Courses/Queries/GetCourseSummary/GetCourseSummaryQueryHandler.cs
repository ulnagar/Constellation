namespace Constellation.Application.Domains.Courses.Queries.GetCourseSummary;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Domains.Courses.Models;
using Constellation.Core.Models.Subjects;
using Constellation.Core.Models.Subjects.Errors;
using Constellation.Core.Shared;
using Core.Models.Faculties;
using Core.Models.Faculties.Errors;
using Core.Models.Faculties.Repositories;
using Core.Models.Subjects.Repositories;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetCourseSummaryQueryHandler
    : IQueryHandler<GetCourseSummaryQuery, CourseSummaryResponse>
{
    private readonly ICourseRepository _courseRepository;
    private readonly IFacultyRepository _facultyRepository;
    private readonly ILogger _logger;

    public GetCourseSummaryQueryHandler(
        ICourseRepository courseRepository,
        IFacultyRepository facultyRepository,
        ILogger logger)
    {
        _courseRepository = courseRepository;
        _facultyRepository = facultyRepository;
        _logger = logger;
    }

    public async Task<Result<CourseSummaryResponse>> Handle(GetCourseSummaryQuery request, CancellationToken cancellationToken)
    {
        Course course = await _courseRepository.GetById(request.CourseId, cancellationToken);

        if (course is null)
        {
            _logger
                .ForContext(nameof(GetCourseSummaryQuery), request, true)
                .ForContext(nameof(Error), CourseErrors.NotFound(request.CourseId), true)
                .Warning("Failed to retrieve Course summary");

            return Result.Failure<CourseSummaryResponse>(CourseErrors.NotFound(request.CourseId));
        }

        Faculty faculty = await _facultyRepository.GetById(course.FacultyId, cancellationToken);

        if (faculty is null)
        {
            _logger
                .ForContext(nameof(GetCourseSummaryQuery), request, true)
                .ForContext(nameof(Error), FacultyErrors.NotFound(course.FacultyId))
                .Warning("Failed to retrieve Course summary");

            return Result.Failure<CourseSummaryResponse>(FacultyErrors.NotFound(course.FacultyId));
        }

        return new CourseSummaryResponse(
            course.Id,
            course.Name,
            course.Code,
            course.Grade,
            new(
                faculty.Id,
                faculty.Name,
                faculty.Colour),
            course.FullTimeEquivalentValue,
            course.TargetMinutesPerCycle,
            new());
    }
}
