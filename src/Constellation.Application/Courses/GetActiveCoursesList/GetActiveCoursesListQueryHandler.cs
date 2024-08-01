namespace Constellation.Application.Courses.GetActiveCoursesList;

using Abstractions.Messaging;
using Constellation.Application.Courses.GetCoursesForSelectionList;
using Constellation.Core.Models.Faculties.Errors;
using Constellation.Core.Models.Faculties.Repositories;
using Constellation.Core.Models.Subjects.Errors;
using Constellation.Core.Models.Subjects;
using Constellation.Core.Models.Subjects.Repositories;
using Core.Models.Faculties;
using Core.Models.Offerings;
using Core.Models.Offerings.Repositories;
using Core.Shared;
using Models;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetActiveCoursesListQueryHandler
: IQueryHandler<GetActiveCoursesListQuery, List<CourseSelectListItemResponse>>
{
    private readonly ICourseRepository _courseRepository;
    private readonly IFacultyRepository _facultyRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly ILogger _logger;

    public GetActiveCoursesListQueryHandler(
        ICourseRepository courseRepository,
        IFacultyRepository facultyRepository,
        IOfferingRepository offeringRepository,
        ILogger logger)
    {
        _courseRepository = courseRepository;
        _facultyRepository = facultyRepository;
        _offeringRepository = offeringRepository;
        _logger = logger
            .ForContext<GetActiveCoursesListQuery>();
    }

    public async Task<Result<List<CourseSelectListItemResponse>>> Handle(GetActiveCoursesListQuery request, CancellationToken cancellationToken)
    {
        List<Course> courses = await _courseRepository.GetAll(cancellationToken);
        
        if (courses.Count == 0)
        {
            _logger.Warning("Could not find any active courses in the database");

            return Result.Failure<List<CourseSelectListItemResponse>>(CourseErrors.NoneFound);
        }

        List<CourseSelectListItemResponse> response = new();

        foreach (Course course in courses)
        {
            Faculty faculty = await _facultyRepository.GetById(course.FacultyId, cancellationToken);

            if (faculty is null)
                _logger
                    .ForContext(nameof(GetCoursesForSelectionListQuery), request, true)
                    .ForContext(nameof(Error), FacultyErrors.NotFound(course.FacultyId), true)
                    .Warning("Could not find faculty linked to course");

            List<Offering> activeOfferings = await _offeringRepository.GetActiveByCourseId(course.Id, cancellationToken);

            if (activeOfferings.Count == 0)
                continue;

            CourseSelectListItemResponse summary = new(
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
