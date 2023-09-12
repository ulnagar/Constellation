namespace Constellation.Application.Courses.GetCoursesForSelectionList;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Errors;
using Constellation.Core.Models;
using Constellation.Core.Models.Subjects;
using Constellation.Core.Shared;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class GetCoursesForSelectionListQueryHandler
    : IQueryHandler<GetCoursesForSelectionListQuery, List<CourseSummaryResponse>>
{
    private readonly ICourseRepository _courseRepository;
    private readonly IFacultyRepository _facultyRepository;
    private readonly ILogger _logger;

    public GetCoursesForSelectionListQueryHandler(
        ICourseRepository courseRepository,
        IFacultyRepository facultyRepository,
        ILogger logger)
    {
        _courseRepository = courseRepository;
        _facultyRepository = facultyRepository;
        _logger = logger.ForContext<GetCoursesForSelectionListQuery>();
    }

    public async Task<Result<List<CourseSummaryResponse>>> Handle(GetCoursesForSelectionListQuery request, CancellationToken cancellationToken)
    {
        List<Course> courses = await _courseRepository.GetAll(cancellationToken);

        if (courses is null)
        {
            _logger.Warning("Could not find any courses in the database");

            return Result.Failure<List<CourseSummaryResponse>>(DomainErrors.Subjects.Course.NotFound(0));
        }

        List<CourseSummaryResponse> response = new();

        foreach (Course course in courses)
        {
            Faculty faculty = await _facultyRepository.GetById(course.FacultyId, cancellationToken);

            if (faculty is null)
                _logger
                    .ForContext(nameof(GetCoursesForSelectionListQuery), request, true)
                    .ForContext(nameof(Error), DomainErrors.Partners.Faculty.NotFound(course.FacultyId), true)
                    .Warning("Could not find faculty linked to course");


            CourseSummaryResponse summary = new(
                course.Id,
                course.Name,
                course.Grade,
                course.FacultyId,
                faculty?.Name,
                $"{course.Grade} {course.Name}");

            response.Add(summary);
        }

        return response
            .OrderBy(summary => summary.FacultyName)
            .ThenBy(summary => summary.DisplayName)
            .ToList();
    }
}
