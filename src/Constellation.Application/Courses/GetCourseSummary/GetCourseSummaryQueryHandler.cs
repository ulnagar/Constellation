namespace Constellation.Application.Courses.GetCourseSummary;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Errors;
using Constellation.Core.Models;
using Constellation.Core.Models.Subjects;
using Constellation.Core.Shared;
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
                .ForContext(nameof(Error), DomainErrors.Subjects.Course.NotFound(request.CourseId), true)
                .Warning("Failed to retrieve Course summary");

            return Result.Failure<CourseSummaryResponse>(DomainErrors.Subjects.Course.NotFound(request.CourseId));
        }

        Faculty faculty = await _facultyRepository.GetById(course.FacultyId, cancellationToken);

        if (faculty is null)
        {
            _logger
                .ForContext(nameof(GetCourseSummaryQuery), request, true)
                .ForContext(nameof(Error), DomainErrors.Partners.Faculty.NotFound(course.FacultyId))
                .Warning("Failed to retrieve Course summary");

            return Result.Failure<CourseSummaryResponse>(DomainErrors.Partners.Faculty.NotFound(course.FacultyId));
        }

        return new CourseSummaryResponse(
            course.Id,
            course.Name,
            course.Code,
            course.Grade,
            course.FacultyId,
            faculty.Name);
    }
}
