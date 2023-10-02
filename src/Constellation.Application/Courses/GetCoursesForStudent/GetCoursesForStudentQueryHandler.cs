namespace Constellation.Application.Courses.GetCoursesForStudent;

using Abstractions.Messaging;
using Constellation.Core.Models.Offerings.Repositories;
using Core.Abstractions.Repositories;
using Core.Models.Offerings;
using Core.Models.Offerings.Errors;
using Core.Models.Subjects;
using Core.Models.Subjects.Errors;
using Core.Models.Subjects.Identifiers;
using Core.Shared;
using Extensions;
using Interfaces.Repositories;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetCoursesForStudentQueryHandler 
    : IQueryHandler<GetCoursesForStudentQuery, List<StudentCourseResponse>>
{
    private readonly ICourseRepository _courseRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly ILogger _logger;

    public GetCoursesForStudentQueryHandler(
        ICourseRepository courseRepository,
        IOfferingRepository offeringRepository,
        ILogger logger)
    {
        _courseRepository = courseRepository;
        _offeringRepository = offeringRepository;
        _logger = logger.ForContext<GetCoursesForStudentQuery>();
    }

    public async Task<Result<List<StudentCourseResponse>>> Handle(GetCoursesForStudentQuery request, CancellationToken cancellationToken)
    {
        List<StudentCourseResponse> response = new();

        List<Offering> offerings = await _offeringRepository.GetByStudentId(request.StudentId, cancellationToken);

        if (offerings.Count == 0)
        {
            _logger
                .ForContext(nameof(GetCoursesForStudentQuery), request, true)
                .ForContext(nameof(Error), OfferingErrors.NotFoundForStudent(request.StudentId), true)
                .Warning("Failed to retrieve Course list for Student");

            return Result.Failure<List<StudentCourseResponse>>(OfferingErrors.NotFoundForStudent(request.StudentId));
        }

        List<CourseId> courseIds = offerings.Select(offering => offering.CourseId).Distinct().ToList();

        foreach (CourseId courseId in courseIds)
        {
            Course course = await _courseRepository.GetById(courseId, cancellationToken);

            if (course is null)
            {
                _logger
                    .ForContext(nameof(GetCoursesForStudentQuery), request, true)
                    .ForContext(nameof(Error), CourseErrors.NotFound(courseId), true)
                    .Information("Failed to retrieve Course list for Student");

                continue;
            }

            response.Add(new(
                course.Id,
                course.Name,
                course.Grade.AsName()));
        }

        return response;
    }
}
