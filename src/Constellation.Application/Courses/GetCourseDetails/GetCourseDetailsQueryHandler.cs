namespace Constellation.Application.Courses.GetCourseDetails;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Clock;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Errors;
using Constellation.Core.Models;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Models.Offerings.ValueObjects;
using Constellation.Core.Models.Subjects;
using Constellation.Core.Models.Subjects.Errors;
using Constellation.Core.Shared;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetCourseDetailsQueryHandler
    : IQueryHandler<GetCourseDetailsQuery, CourseDetailsResponse>
{
    private readonly ICourseRepository _courseRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IFacultyRepository _facultyRepository;
    private readonly IEnrolmentRepository _enrolmentRepository;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger _logger;

    public GetCourseDetailsQueryHandler(
        ICourseRepository courseRepository,
        IOfferingRepository offeringRepository,
        IStaffRepository staffRepository,
        IFacultyRepository facultyRepository,
        IEnrolmentRepository enrolmentRepository,
        IDateTimeProvider dateTime,
        ILogger logger)
    {
        _courseRepository = courseRepository;
        _offeringRepository = offeringRepository;
        _staffRepository = staffRepository;
        _facultyRepository = facultyRepository;
        _enrolmentRepository = enrolmentRepository;
        _dateTime = dateTime;
        _logger = logger.ForContext<GetCourseDetailsQueryHandler>();
    }

    public async Task<Result<CourseDetailsResponse>> Handle(GetCourseDetailsQuery request, CancellationToken cancellationToken)
    {
        Course course = await _courseRepository.GetById(request.CourseId, cancellationToken);

        if (course is null)
        {
            _logger
                .ForContext(nameof(GetCourseDetailsQuery), request, true)
                .ForContext(nameof(Error), CourseErrors.NotFound(request.CourseId), true)
                .Warning("Could not retrieve Course details");

            return Result.Failure<CourseDetailsResponse>(CourseErrors.NotFound(request.CourseId));
        }

        Faculty faculty = await _facultyRepository.GetById(course.FacultyId, cancellationToken);

        if (faculty is null)
        {
            _logger
                .ForContext(nameof(GetCourseDetailsQuery), request, true)
                .ForContext(nameof(Error), DomainErrors.Partners.Faculty.NotFound(course.FacultyId))
                .Warning("Failed to retrieve Course details");

            return Result.Failure<CourseDetailsResponse>(DomainErrors.Partners.Faculty.NotFound(course.FacultyId));
        }

        CourseDetailsResponse.Faculty responseFaculty = new(
            faculty.Id,
            faculty.Name,
            faculty.Colour);

        List<Offering> offerings = await _offeringRepository.GetByCourseId(course.Id, cancellationToken);

        List<CourseDetailsResponse.Offering> responseOfferings = new();

        foreach (Offering offering in offerings)
        {
            List<string> teacherIds = offering
                .Teachers
                .Where(assignment => assignment.Type == AssignmentType.ClassroomTeacher)
                .Where(assignment => !assignment.IsDeleted)
                .Select(assignment => assignment.StaffId)
                .ToList();

            List<Staff> teachers = await _staffRepository.GetListFromIds(teacherIds, cancellationToken);

            List<CourseDetailsResponse.Teacher> responseTeachers = new();

            foreach (Staff teacher in teachers)
            {
                responseTeachers.Add(new(
                    teacher.StaffId,
                    teacher.DisplayName));
            }

            responseOfferings.Add(new(
                offering.Id,
                offering.Name,
                responseTeachers,
                offering.EndDate,
                offering.IsCurrent,
                offering.StartDate > _dateTime.Today));
        }

        int enrolments = await _enrolmentRepository.GetCurrentCountByCourseId(course.Id, cancellationToken);

        return new CourseDetailsResponse(
            course.Id,
            course.Name,
            course.Code,
            course.Grade,
            responseFaculty,
            responseOfferings,
            course.FullTimeEquivalentValue,
            enrolments * course.FullTimeEquivalentValue);
    }
}
