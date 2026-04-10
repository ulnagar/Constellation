namespace Constellation.Application.Domains.Contacts.Queries.GetContactListForStudent;

using Abstractions.Messaging;
using Application.Interfaces.Repositories;
using Core.Abstractions.Repositories;
using Core.Errors;
using Core.Models;
using Core.Models.Faculties;
using Core.Models.Faculties.Enums;
using Core.Models.Faculties.Repositories;
using Core.Models.Families;
using Core.Models.Offerings;
using Core.Models.Offerings.Enums;
using Core.Models.Offerings.Repositories;
using Core.Models.Offerings.ValueObjects;
using Core.Models.SchoolContacts;
using Core.Models.SchoolContacts.Enums;
using Core.Models.SchoolContacts.Repositories;
using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Identifiers;
using Core.Models.StaffMembers.Repositories;
using Core.Models.Students;
using Core.Models.Students.Errors;
using Core.Models.Students.Repositories;
using Core.Models.Subjects;
using Core.Models.Subjects.Repositories;
using Core.Shared;
using Core.ValueObjects;
using Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetContactListForStudentQueryHandler
: IQueryHandler<GetContactListForStudentQuery, List<ContactResponse>>
{
    private readonly IStudentRepository _studentRepository;
    private readonly ISchoolRepository _schoolRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IFacultyRepository _facultyRepository;
    private readonly IFamilyRepository _familyRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly ISchoolContactRepository _contactRepository;

    public GetContactListForStudentQueryHandler(
        IStudentRepository studentRepository,
        ISchoolRepository schoolRepository,
        IOfferingRepository offeringRepository,
        ICourseRepository courseRepository,
        IFacultyRepository facultyRepository,
        IFamilyRepository familyRepository,
        IStaffRepository staffRepository,
        ISchoolContactRepository contactRepository)
    {
        _studentRepository = studentRepository;
        _schoolRepository = schoolRepository;
        _offeringRepository = offeringRepository;
        _courseRepository = courseRepository;
        _facultyRepository = facultyRepository;
        _familyRepository = familyRepository;
        _staffRepository = staffRepository;
        _contactRepository = contactRepository;
    }

    public async Task<Result<List<ContactResponse>>> Handle(GetContactListForStudentQuery request, CancellationToken cancellationToken)
    {
        List<ContactResponse> result = new();

        Student? student = await _studentRepository.GetById(request.StudentId, cancellationToken);

        if (student is null)
            return Result.Failure<List<ContactResponse>>(StudentErrors.NotFound(request.StudentId));

        SchoolEnrolment? enrolment = student.CurrentEnrolment;

        if (enrolment is null)
            return Result.Failure<List<ContactResponse>>(SchoolEnrolmentErrors.NotFound);
            
        School? school = await _schoolRepository.GetById(enrolment.SchoolCode, cancellationToken);

        if (school is null)
            return Result.Failure<List<ContactResponse>>(DomainErrors.Partners.School.NotFound(enrolment.SchoolCode));

        List<StaffMember> staffMembers = await _staffRepository
            .GetAll(cancellationToken);

        List<Faculty> faculties = await _facultyRepository
            .GetAll(cancellationToken);

        List<Course> courses = await _courseRepository
            .GetAll(cancellationToken);

        result.Add(new(
            student.StudentReferenceNumber,
            student.Name,
            enrolment.Grade,
            enrolment.SchoolName,
            ContactCategory.Student,
            student.Id,
            student.Name.DisplayName,
            student.EmailAddress,
            null,
            string.Empty));

        Result<PhoneNumber> schoolPhone = PhoneNumber.Create(school.PhoneNumber);

        Result<EmailAddress> schoolEmail = EmailAddress.Create(school.EmailAddress);

        if (schoolEmail.IsSuccess)
        {
            result.Add(new(
                student.StudentReferenceNumber,
                student.Name,
                enrolment.Grade,
                enrolment.SchoolName,
                ContactCategory.PartnerSchoolSchool,
                school.Code,
                enrolment.SchoolName,
                schoolEmail.Value,
                schoolPhone.IsSuccess ? schoolPhone.Value : null,
                string.Empty));
        }

        List<SchoolContact> contacts = await _contactRepository.GetWithRolesBySchool(enrolment.SchoolCode, cancellationToken);

        foreach (SchoolContact contact in contacts)
        {
            foreach (SchoolContactRole role in contact.Assignments.Where(role => role.SchoolCode == enrolment.SchoolCode))
            {
                ContactCategory category = role switch
                {
                    _ when role.Role == Position.Principal => ContactCategory.PartnerSchoolPrincipal,
                    _ when role.Role == Position.Coordinator => ContactCategory.PartnerSchoolACC,
                    _ when role.Role == Position.SciencePracticalTeacher => ContactCategory.PartnerSchoolSPT,
                    _ => ContactCategory.PartnerSchoolOtherStaff
                };

                result.Add(new(
                    student.StudentReferenceNumber,
                    student.Name,
                    enrolment.Grade,
                    enrolment.SchoolName,
                    category,
                    contact.Id,
                    contact.Name.DisplayName,
                    contact.EmailAddress,
                    contact.PhoneNumber,
                    role.Note));
            }
        }

        List<Family> families = await _familyRepository.GetFamiliesByStudentId(student.Id, cancellationToken);

        foreach (Family family in families)
        {
            Result<EmailAddress> familyEmail = EmailAddress.Create(family.FamilyEmail);

            if (familyEmail.IsFailure)
                continue;

            bool isResidential = family.Students.First(entry => entry.StudentId == student.Id).IsResidentialFamily;

            if (isResidential)
            {
                result.Add(new(
                    student.StudentReferenceNumber,
                    student.Name,
                    enrolment.Grade,
                    enrolment.SchoolName,
                    ContactCategory.ResidentialFamily,
                    family.Id,
                    family.FamilyTitle,
                    familyEmail.Value,
                    null,
                    string.Empty));

                foreach (Parent parent in family.Parents)
                {
                    ContactCategory category = parent.SentralLink switch
                    {
                        Parent.SentralReference.Father => ContactCategory.ResidentialFather,
                        Parent.SentralReference.Mother => ContactCategory.ResidentialMother,
                        _ => ContactCategory.ResidentialFamily
                    };

                    result.Add(new(
                        student.StudentReferenceNumber,
                        student.Name,
                        enrolment.Grade,
                        enrolment.SchoolName,
                        category,
                        parent.Id,
                        parent.Name.DisplayName,
                        parent.EmailAddress,
                        parent.MobileNumber,
                        string.Empty));
                }
            }
            else
            {
                result.Add(new(
                    student.StudentReferenceNumber,
                    student.Name,
                    enrolment.Grade,
                    enrolment.SchoolName,
                    ContactCategory.NonResidentialFamily,
                    family.Id,
                    family.FamilyTitle,
                    familyEmail.Value,
                    null,
                    string.Empty));

                foreach (Parent parent in family.Parents)
                {
                    result.Add(new(
                        student.StudentReferenceNumber,
                        student.Name,
                        enrolment.Grade,
                        enrolment.SchoolName,
                        ContactCategory.NonResidentialParent,
                        parent.Id,
                        parent.Name.DisplayName,
                        parent.EmailAddress,
                        parent.MobileNumber,
                        string.Empty));
                }
            }
        }

        List<Offering> studentOfferings = await _offeringRepository.GetByStudentId(student.Id, cancellationToken);

        foreach (Offering offering in studentOfferings)
        {
            List<StaffId> staffIds = offering
                .Teachers
                .Where(teacher =>
                    !teacher.IsDeleted &&
                    teacher.Type == AssignmentType.ClassroomTeacher)
                .Select(entry => entry.StaffId)
                .ToList();

            List<StaffMember> teachers = staffMembers.Where(entry => staffIds.Contains(entry.Id)).ToList();

            foreach (StaffMember teacher in teachers)
            {
                string teacherName = teacher.Name.DisplayName;
                teacherName += $" ({offering.Name})";

                result.Add(new(
                    student.StudentReferenceNumber,
                    student.Name,
                    enrolment.Grade,
                    enrolment.SchoolName,
                    ContactCategory.AuroraTeacher,
                    teacher.Id,
                    teacherName,
                    teacher.EmailAddress,
                    null,
                    string.Empty));
            }

            Course course = courses.First(entry => entry.Id == offering.CourseId);

            Faculty faculty = faculties.First(entry => entry.Id == course.FacultyId);

            List<StaffId> headTeacherIds = faculty
                .Members
                .Where(member =>
                    !member.IsDeleted &&
                    member.Role == FacultyMembershipRole.Manager)
                .Select(member => member.StaffId)
                .ToList();

            teachers = staffMembers
                .Where(entry => headTeacherIds.Contains(entry.Id))
                .ToList();

            foreach (StaffMember headTeacher in teachers)
            {
                string teacherName = headTeacher.Name.DisplayName;
                teacherName += $" ({faculty.Name})";

                bool existingEntry = result.Any(entry =>
                    entry.Category.Equals(ContactCategory.AuroraHeadTeacher) &&
                entry.Contact == teacherName &&
                    entry.StudentId == student.StudentReferenceNumber);

                if (existingEntry)
                    continue;

                result.Add(new(
                    student.StudentReferenceNumber,
                    student.Name,
                    enrolment.Grade,
                    enrolment.SchoolName,
                    ContactCategory.AuroraHeadTeacher,
                    headTeacher.Id,
                    teacherName,
                    headTeacher.EmailAddress,
                    null,
                    string.Empty));
            }
        }

        return result;
    }
}
