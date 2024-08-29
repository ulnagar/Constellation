namespace Constellation.Application.Schools.GetSchoolDetails;

using Abstractions.Messaging;
using Constellation.Core.Models.Enrolments.Repositories;
using Core.Errors;
using Core.Models;
using Core.Models.Enrolments;
using Core.Models.Faculties;
using Core.Models.Faculties.Repositories;
using Core.Models.Offerings;
using Core.Models.Offerings.Repositories;
using Core.Models.Offerings.ValueObjects;
using Core.Models.SchoolContacts;
using Core.Models.SchoolContacts.Repositories;
using Core.Models.StaffMembers.Repositories;
using Core.Models.Students;
using Core.Models.Students.Repositories;
using Core.Shared;
using Core.ValueObjects;
using Interfaces.Repositories;
using Serilog;
using Students.GetCurrentStudentsWithCurrentOfferings;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetSchoolDetailsQueryHandler
: IQueryHandler<GetSchoolDetailsQuery, SchoolDetailsResponse>
{
    private readonly ISchoolRepository _schoolRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IFacultyRepository _facultyRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly ISchoolContactRepository _contactRepository;
    private readonly IEnrolmentRepository _enrolmentRepository;
    private readonly ILogger _logger;

    public GetSchoolDetailsQueryHandler(
        ISchoolRepository schoolRepository,
        IStaffRepository staffRepository,
        IFacultyRepository facultyRepository,
        IStudentRepository studentRepository,
        IOfferingRepository offeringRepository,
        ISchoolContactRepository contactRepository,
        IEnrolmentRepository enrolmentRepository,
        ILogger logger)
    {
        _schoolRepository = schoolRepository;
        _staffRepository = staffRepository;
        _facultyRepository = facultyRepository;
        _studentRepository = studentRepository;
        _offeringRepository = offeringRepository;
        _contactRepository = contactRepository;
        _enrolmentRepository = enrolmentRepository;
        _logger = logger.ForContext<GetSchoolDetailsQuery>();
    }

    public async Task<Result<SchoolDetailsResponse>> Handle(GetSchoolDetailsQuery request, CancellationToken cancellationToken)
    {
        School school = await _schoolRepository.GetById(request.SchoolCode, cancellationToken);

        if (school is null)
        {
            _logger
                .ForContext(nameof(GetSchoolDetailsQuery), request, true)
                .ForContext(nameof(Error), DomainErrors.Partners.School.NotFound(request.SchoolCode), true)
                .Warning("Failed to retrieve School details");

            return Result.Failure<SchoolDetailsResponse>(DomainErrors.Partners.School.NotFound(request.SchoolCode));
        }

        List<Student> students = await _studentRepository.GetCurrentStudentsFromSchool(school.Code, cancellationToken);
        
        List<StudentWithOfferingsResponse> studentResponse = new();

        foreach (Student student in students)
        {
            List<StudentWithOfferingsResponse.OfferingResponse> studentOfferings = new();

            List<Enrolment> enrolments = await _enrolmentRepository.GetCurrentByStudentId(student.StudentId, cancellationToken);

            foreach (Enrolment enrolment in enrolments)
            {
                Offering offering = await _offeringRepository.GetById(enrolment.OfferingId, cancellationToken);

                if (offering is null)
                    continue;

                studentOfferings.Add(new(
                    offering.Id,
                    offering.Name,
                    offering.IsCurrent));
            }

            studentResponse.Add(new(
                student.StudentId,
                student.GetName(),
                student.Gender,
                student.School.Name,
                student.CurrentGrade,
                studentOfferings));
        }

        List<Staff> teachers = await _staffRepository.GetActiveFromSchool(school.Code, cancellationToken);
        List<Faculty> faculties = await _facultyRepository.GetAll(cancellationToken);
        List<SchoolDetailsResponse.SchoolStaff> staffResponse = new();

        foreach (Staff teacher in teachers)
        {
            List<Offering> offerings = await _offeringRepository.GetActiveForTeacher(teacher.StaffId, cancellationToken);

            List<SchoolDetailsResponse.OfferingResponse> teacherOfferings = offerings
                .Where(offering => 
                    offering.Teachers.Any(entry => 
                        entry.StaffId == teacher.StaffId && 
                        entry.Type == AssignmentType.ClassroomTeacher))
                .Select(offering => new SchoolDetailsResponse.OfferingResponse(
                    offering.Id,
                    offering.Name,
                    offering.IsCurrent))
                .ToList();

            Dictionary<string, string> teacherFaculties = faculties
                .Where(faculty => teacher.Faculties
                    .Where(membership => !membership.IsDeleted)
                    .Select(membership => membership.FacultyId)
                    .Contains(faculty.Id))
                .ToDictionary(key => key.Name, value => value.Colour);

            staffResponse.Add(new(
                teacher.StaffId,
                teacher.GetName(),
                teacherFaculties,
                teacherOfferings));
        }

        List<SchoolContact> contacts = await _contactRepository.GetWithRolesBySchool(school.Code, cancellationToken);
        List<SchoolDetailsResponse.SchoolContact> contactResponse = new();

        foreach (SchoolContact contact in contacts)
        {
            foreach (SchoolContactRole role in contact.Assignments.Where(role => !role.IsDeleted && role.SchoolCode == school.Code))
            {
                Result<Name> name = contact.GetName();

                if (name.IsFailure)
                {
                    _logger
                        .ForContext(nameof(Error), name.Error, true)
                        .ForContext(nameof(SchoolContact), contact, true)
                        .ForContext(nameof(SchoolContactRole), role, true)
                        .Warning("Failed to retrieve School details");
                }

                contactResponse.Add(new(
                    contact.Id,
                    role.Id,
                    name.Value,
                    role.Role,
                    role.Note,
                    string.IsNullOrWhiteSpace(contact.PhoneNumber) ? PhoneNumber.Empty : PhoneNumber.Create(contact.PhoneNumber).Value,
                    string.IsNullOrWhiteSpace(contact.EmailAddress) ? EmailAddress.None : EmailAddress.Create(contact.EmailAddress).Value));
            }
        }

        return new SchoolDetailsResponse(
            school.Code,
            school.Name,
            school.Address,
            school.Town,
            school.State,
            school.PostCode,
            school.PhoneNumber,
            school.FaxNumber,
            school.EmailAddress,
            school.HeatSchool,
            school.Division,
            school.PrincipalNetwork,
            school.Electorate,
            staffResponse,
            studentResponse,
            contactResponse);
    }
}
