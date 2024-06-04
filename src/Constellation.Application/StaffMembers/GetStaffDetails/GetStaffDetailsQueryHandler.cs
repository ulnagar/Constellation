﻿namespace Constellation.Application.StaffMembers.GetStaffDetails;

using Abstractions.Messaging;
using Core.Errors;
using Core.Models;
using Core.Models.Faculties;
using Core.Models.Faculties.Errors;
using Core.Models.Faculties.Repositories;
using Core.Models.Offerings;
using Core.Models.Offerings.Errors;
using Core.Models.Offerings.Repositories;
using Core.Models.Offerings.ValueObjects;
using Core.Models.SchoolContacts;
using Core.Models.SchoolContacts.Errors;
using Core.Models.SchoolContacts.Repositories;
using Core.Models.StaffMembers.Repositories;
using Core.Models.Subjects;
using Core.Models.Subjects.Repositories;
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
    private readonly ITimetablePeriodRepository _periodRepository;
    private readonly ILogger _logger;

    public GetStaffDetailsQueryHandler(
        IStaffRepository staffRepository,
        ISchoolRepository schoolRepository,
        IOfferingRepository offeringRepository,
        ICourseRepository courseRepository,
        IFacultyRepository facultyRepository,
        ISchoolContactRepository contactRepository,
        ITimetablePeriodRepository periodRepository,
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
        Staff staffMember = await _staffRepository.GetById(request.StaffId, cancellationToken);

        if (staffMember is null)
        {
            _logger
                .ForContext(nameof(GetStaffDetailsQuery), request, true)
                .ForContext(nameof(Error), DomainErrors.Partners.Staff.NotFound(request.StaffId), true)
                .Warning("Failed to retrieve details of staff member");

            return Result.Failure<StaffDetailsResponse>(DomainErrors.Partners.Staff.NotFound(request.StaffId));
        }

        School school = await _schoolRepository.GetById(staffMember.SchoolCode, cancellationToken);

        if (school is null)
        {
            _logger
                .ForContext(nameof(GetStaffDetailsQuery), request, true)
                .ForContext(nameof(Error), DomainErrors.Partners.School.NotFound(staffMember.SchoolCode), true)
                .Warning("Failed to retrieve details of staff member");

            return Result.Failure<StaffDetailsResponse>(DomainErrors.Partners.School.NotFound(staffMember.SchoolCode));
        }

        List<Faculty> faculties = await _facultyRepository.GetCurrentForStaffMember(request.StaffId, cancellationToken);

        if (faculties.Count == 0)
        {
            _logger
                .ForContext(nameof(GetStaffDetailsQuery), request, true)
                .ForContext(nameof(Error), FacultyErrors.NotFoundForStaff(request.StaffId), true)
                .Warning("Failed to retrieve details of staff member");

            return Result.Failure<StaffDetailsResponse>(FacultyErrors.NotFoundForStaff(request.StaffId));
        }

        List<StaffDetailsResponse.FacultyMembershipResponse> facultyMemberships = new();

        foreach (Faculty faculty in faculties)
        {
            FacultyMembership membership = staffMember.Faculties.FirstOrDefault(entry => entry.FacultyId == faculty.Id);

            if (membership is null)
                continue;

            facultyMemberships.Add(new(
                membership.Id,
                faculty.Id,
                faculty.Name,
                membership.Role));
        }

        List<Offering> offerings = await _offeringRepository.GetActiveForTeacher(request.StaffId, cancellationToken);

        if (offerings.Count == 0)
        {
            _logger
                .ForContext(nameof(GetStaffDetailsQuery), request, true)
                .ForContext(nameof(Error), OfferingErrors.NotFoundForTeacher(request.StaffId), true)
                .Warning("Failed to retrieve details of staff member");

            return Result.Failure<StaffDetailsResponse>(OfferingErrors.NotFoundForTeacher(request.StaffId));
        }

        List<TimetablePeriod> periods = await _periodRepository.GetAll(cancellationToken);
        List<Course> courses = await _courseRepository.GetAll(cancellationToken);

        List<StaffDetailsResponse.OfferingResponse> linkedOfferings = new();
        List<StaffDetailsResponse.SessionResponse> linkedSessions = new();

        foreach (Offering offering in offerings)
        {
            Course? course = courses.FirstOrDefault(course => course.Id == offering.CourseId);

            if (course is null)
                continue;

            TeacherAssignment assignment = offering.Teachers.FirstOrDefault(entry => entry.StaffId == staffMember.StaffId);

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
                TimetablePeriod period = periods.FirstOrDefault(entry => entry.Id == session.PeriodId);

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

        List<SchoolContact> contacts = await _contactRepository.GetWithRolesBySchool(staffMember.SchoolCode, cancellationToken);

        if (contacts.Count == 0)
        {
            _logger
                .ForContext(nameof(GetStaffDetailsQuery), request, true)
                .ForContext(nameof(Error), SchoolContactRoleErrors.NotFoundForSchool(staffMember.SchoolCode), true)
                .Warning("Failed to retrieve details of staff member");

            return Result.Failure<StaffDetailsResponse>(SchoolContactRoleErrors.NotFoundForSchool(staffMember.SchoolCode));
        }

        List<StaffDetailsResponse.SchoolContactResponse> schoolContacts = new();

        foreach (SchoolContact contact in contacts)
        {
            List<SchoolContactRole> assignments = contact.Assignments
                .Where(entry => 
                    entry.SchoolCode == staffMember.SchoolCode && 
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

        return new StaffDetailsResponse(
            staffMember.StaffId,
            staffMember.GetName(),
            staffMember.EmailAddress,
            school.Name,
            school.Code,
            staffMember.IsShared,
            staffMember.IsDeleted,
            facultyMemberships,
            linkedOfferings,
            linkedSessions,
            schoolContacts);
    }
}