namespace Constellation.Application.Courses.GetCourseSummaryList;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Courses.GetCourseSummary;
using Constellation.Application.Courses.Models;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models.Faculty;
using Constellation.Core.Models.Faculty.Repositories;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Models.Subjects;
using Constellation.Core.Shared;
using Core.Models.Faculty.Errors;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetCourseSummaryListQueryHandler
    : IQueryHandler<GetCourseSummaryListQuery, List<CourseSummaryResponse>>
{
    private readonly ICourseRepository _courseRepository;
    private readonly IFacultyRepository _facultyRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly ILogger _logger;

    public GetCourseSummaryListQueryHandler(
        ICourseRepository courseRepository,
        IFacultyRepository facultyRepository,
        IOfferingRepository offeringRepository,
        ILogger logger)
    {
        _courseRepository = courseRepository;
        _facultyRepository = facultyRepository;
        _offeringRepository = offeringRepository;
        _logger = logger.ForContext<GetCourseSummaryListQuery>();
    }

    public async Task<Result<List<CourseSummaryResponse>>> Handle(GetCourseSummaryListQuery request, CancellationToken cancellationToken)
    {
        List<CourseSummaryResponse> responses = new();

        List<Course> courses = await _courseRepository.GetAll(cancellationToken);
        List<Faculty> faculties = await _facultyRepository.GetAll(cancellationToken);

        foreach (Course course in courses)
        {
            Faculty faculty = faculties.FirstOrDefault(faculty => faculty.Id == course.FacultyId);

            if (faculty is null)
            {
                _logger
                    .ForContext(nameof(GetCourseSummaryQuery), request, true)
                    .ForContext(nameof(Error), FacultyErrors.NotFound(course.FacultyId))
                    .Warning("Failed to retrieve Course summary");

                return Result.Failure<List<CourseSummaryResponse>>(FacultyErrors.NotFound(course.FacultyId));
            }

            CourseSummaryResponse.Faculty responseFaculty = new(
                faculty.Id,
                faculty.Name,
                faculty.Colour);

            List<Offering> offerings = await _offeringRepository.GetByCourseId(course.Id, cancellationToken);

            List<CourseSummaryResponse.Offering> responseOfferings = new();

            foreach (Offering offering in offerings)
            {
                responseOfferings.Add(new(
                    offering.Id,
                    offering.Name,
                    offering.IsCurrent));
            }

            responses.Add(new(
                course.Id,
                course.Name,
                course.Code,
                course.Grade,
                responseFaculty,
                course.FullTimeEquivalentValue,
                responseOfferings));
        }

        return responses;
    }
}
