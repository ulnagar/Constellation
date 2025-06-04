namespace Constellation.Application.Domains.Courses.Queries.GetCourseDetails;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Abstractions.Clock;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Models.Offerings.ValueObjects;
using Constellation.Core.Models.Subjects;
using Constellation.Core.Models.Subjects.Errors;
using Constellation.Core.Shared;
using Core.Models.Enrolments.Repositories;
using Core.Models.Faculties;
using Core.Models.Faculties.Errors;
using Core.Models.Faculties.Repositories;
using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Identifiers;
using Core.Models.StaffMembers.Repositories;
using Core.Models.Subjects.Repositories;
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
                .ForContext(nameof(Error), FacultyErrors.NotFound(course.FacultyId))
                .Warning("Failed to retrieve Course details");

            return Result.Failure<CourseDetailsResponse>(FacultyErrors.NotFound(course.FacultyId));
        }

        CourseDetailsResponse.Faculty responseFaculty = new(
            faculty.Id,
            faculty.Name,
            faculty.Colour);

        List<Offering> offerings = await _offeringRepository.GetByCourseId(course.Id, cancellationToken);

        List<CourseDetailsResponse.Offering> responseOfferings = new();

        foreach (Offering offering in offerings)
        {
            List<StaffId> teacherIds = offering
                .Teachers
                .Where(assignment => assignment.Type == AssignmentType.ClassroomTeacher)
                .Where(assignment => !assignment.IsDeleted)
                .Select(assignment => assignment.StaffId)
                .ToList();

            List<StaffMember> teachers = await _staffRepository.GetListFromIds(teacherIds, cancellationToken);

            List<CourseDetailsResponse.Teacher> responseTeachers = new();

            foreach (StaffMember teacher in teachers)
            {
                responseTeachers.Add(new(
                    teacher.Id,
                    teacher.Name));
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
