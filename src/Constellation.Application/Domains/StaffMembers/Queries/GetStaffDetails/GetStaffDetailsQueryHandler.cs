namespace Constellation.Application.Domains.StaffMembers.Queries.GetStaffDetails;

using Abstractions.Messaging;
using Core.Models;
using Core.Models.Faculties;
using Core.Models.Faculties.Repositories;
using Core.Models.Offerings;
using Core.Models.Offerings.Repositories;
using Core.Models.Offerings.ValueObjects;
using Core.Models.SchoolContacts;
using Core.Models.SchoolContacts.Repositories;
using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Errors;
using Core.Models.StaffMembers.Repositories;
using Core.Models.Subjects;
using Core.Models.Subjects.Repositories;
using Core.Models.Timetables;
using Core.Models.Timetables.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetStaffDetailsQueryHandler
: IQueryHandler<GetStaffDetailsQuery, StaffDetailsResponse>
{
    private readonly IStaffRepository _staffRepository;
    private readonly ISchoolRepository _schoolRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IFacultyRepository _facultyRepository;
    private readonly ISchoolContactRepository _contactRepository;
    private readonly IPeriodRepository _periodRepository;
    private readonly ILogger _logger;

    public GetStaffDetailsQueryHandler(
        IStaffRepository staffRepository,
        ISchoolRepository schoolRepository,
        IOfferingRepository offeringRepository,
        ICourseRepository courseRepository,
        IFacultyRepository facultyRepository,
        ISchoolContactRepository contactRepository,
        IPeriodRepository periodRepository,
        ILogger logger)
    {
        _staffRepository = staffRepository;
        _schoolRepository = schoolRepository;
        _offeringRepository = offeringRepository;
        _courseRepository = courseRepository;
        _facultyRepository = facultyRepository;
        _contactRepository = contactRepository;
        _periodRepository = periodRepository;
        _logger = logger.ForContext<GetStaffDetailsQuery>();
    }

    public async Task<Result<StaffDetailsResponse>> Handle(GetStaffDetailsQuery request, CancellationToken cancellationToken)
    {
        StaffMember staffMember = await _staffRepository.GetById(request.StaffId, cancellationToken);

        if (staffMember is null)
        {
            _logger
                .ForContext(nameof(GetStaffDetailsQuery), request, true)
                .ForContext(nameof(Error), StaffMemberErrors.NotFound(request.StaffId), true)
                .Warning("Failed to retrieve details of staff member");

            return Result.Failure<StaffDetailsResponse>(StaffMemberErrors.NotFound(request.StaffId));
        }

        School school = staffMember.CurrentAssignment is not null
            ? await _schoolRepository.GetById(staffMember.CurrentAssignment.SchoolCode, cancellationToken)
            : null;

        List<StaffDetailsResponse.SchoolAssignmentResponse> schoolAssignments = new();

        foreach (SchoolAssignment assignment in staffMember.SchoolAssignments)
        {
            schoolAssignments.Add(new(
                assignment.Id,
                assignment.SchoolCode,
                assignment.SchoolName,
                assignment.IsDeleted,
                assignment.StartDate,
                assignment.EndDate));
        }

        List<Faculty> faculties = await _facultyRepository.GetCurrentForStaffMember(request.StaffId, cancellationToken);

        List<StaffDetailsResponse.FacultyMembershipResponse> facultyMemberships = new();

        foreach (Faculty faculty in faculties)
        {
            FacultyMembership membership = faculty.Members
                .FirstOrDefault(membership => membership.StaffId == staffMember.Id);

            facultyMemberships.Add(new(
                membership.Id,
                membership.FacultyId,
                faculty.Name,
                membership.Role));
        }

        List<Offering> offerings = await _offeringRepository.GetActiveForTeacher(request.StaffId, cancellationToken);
        
        List<Period> periods = await _periodRepository.GetAll(cancellationToken);
        List<Course> courses = await _courseRepository.GetAll(cancellationToken);

        List<StaffDetailsResponse.OfferingResponse> linkedOfferings = new();
        List<StaffDetailsResponse.SessionResponse> linkedSessions = new();

        foreach (Offering offering in offerings)
        {
            Course? course = courses.FirstOrDefault(course => course.Id == offering.CourseId);

            if (course is null)
                continue;

            TeacherAssignment assignment = offering.Teachers.FirstOrDefault(entry => entry.StaffId == staffMember.Id);

            if (assignment is null)
                continue;

            linkedOfferings.Add(new(
                offering.Id,
                offering.Name,
                course.ToString(),
                assignment.Type.Value));

            if (assignment.Type != AssignmentType.ClassroomTeacher)
                continue;

            foreach (Session session in offering.Sessions.Where(session => !session.IsDeleted))
            {
                Period period = periods.FirstOrDefault(entry => entry.Id == session.PeriodId);

                if (period is null)
                    continue;

                linkedSessions.Add(new(
                    session.Id,
                    period.ToString(),
                    period.SortOrder,
                    offering.Name,
                    period.Duration));
            }
        }

        List<StaffDetailsResponse.SchoolContactResponse> schoolContacts = new();
        
        if (school is not null)
        {
            List<SchoolContact> contacts = await _contactRepository.GetWithRolesBySchool(school.Code, cancellationToken);


            foreach (SchoolContact contact in contacts)
            {
                List<SchoolContactRole> assignments = contact.Assignments
                    .Where(entry =>
                        entry.SchoolCode == school.Code &&
                        !entry.IsDeleted)
                    .ToList();

                foreach (SchoolContactRole assignment in assignments)
                {
                    schoolContacts.Add(new(
                        contact.Id,
                        contact.DisplayName,
                        contact.EmailAddress,
                        string.IsNullOrWhiteSpace(contact.PhoneNumber) ? school.PhoneNumber : contact.PhoneNumber,
                        assignment.Role,
                        school.Name));
                }
            }
        }

        return new StaffDetailsResponse(
            staffMember.Id,
            staffMember.EmployeeId,
            staffMember.Name,
            staffMember.EmailAddress,
            staffMember.IsShared,
            staffMember.IsDeleted,
            schoolAssignments,
            facultyMemberships,
            linkedOfferings,
            linkedSessions,
            schoolContacts);
    }
}