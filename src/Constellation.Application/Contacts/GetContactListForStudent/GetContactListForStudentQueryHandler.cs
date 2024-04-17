namespace Constellation.Application.Contacts.GetContactListForStudent;

using Abstractions.Messaging;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models.Faculty.Repositories;
using Constellation.Core.Models.Faculty.ValueObjects;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Models.Offerings.ValueObjects;
using Constellation.Core.Models.SchoolContacts;
using Constellation.Core.Models.SchoolContacts.Repositories;
using Constellation.Core.Models.StaffMembers.Repositories;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Repositories;
using Constellation.Core.Models.Subjects;
using Constellation.Core.Models.Subjects.Repositories;
using Core.Models;
using Core.Models.Faculty;
using Core.Models.Families;
using Core.Models.Offerings;
using Core.Shared;
using Core.ValueObjects;
using Interfaces.Repositories;
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

        Student student = await _studentRepository.GetById(request.StudentId, cancellationToken);

        School school = await _schoolRepository.GetById(student.SchoolCode, cancellationToken);

        List<Staff> staffMembers = await _staffRepository
            .GetAll(cancellationToken);

        List<Faculty> faculties = await _facultyRepository
            .GetAll(cancellationToken);

        List<Course> courses = await _courseRepository
            .GetAll(cancellationToken);

        Name studentName = student.GetName();

        Result<EmailAddress> studentEmail = EmailAddress.Create(student.EmailAddress);

        result.Add(new ContactResponse(
            student.StudentId,
            studentName,
            student.CurrentGrade,
            school!.Name,
            ContactCategory.Student,
            studentName.DisplayName,
            studentEmail.Value,
            null));

        Result<PhoneNumber> schoolPhone = PhoneNumber.Create(student.School.PhoneNumber);

        Result<EmailAddress> schoolEmail = EmailAddress.Create(student.School.EmailAddress);

        if (schoolEmail.IsSuccess)
        {
            result.Add(new ContactResponse(
                student.StudentId,
                studentName,
                student.CurrentGrade,
                student.School.Name,
                ContactCategory.PartnerSchoolSchool,
                student.School.Name,
                schoolEmail.Value,
                schoolPhone.IsSuccess ? schoolPhone.Value : null));
        }

        List<SchoolContact> contacts = await _contactRepository.GetWithRolesBySchool(student.SchoolCode, cancellationToken);

        foreach (SchoolContact contact in contacts)
        {
            Result<Name> contactName = Name.Create(contact.FirstName, null, contact.LastName);
            if (contactName.IsFailure)
                continue;

            Result<EmailAddress> contactEmail = EmailAddress.Create(contact.EmailAddress);

            if (contactEmail.IsFailure)
                continue;

            Result<PhoneNumber> contactPhone = PhoneNumber.Create(contact.PhoneNumber);

            foreach (SchoolContactRole role in contact.Assignments.Where(role => role.SchoolCode == student.SchoolCode))
            {
                ContactCategory category = role.Role switch
                {
                    SchoolContactRole.Principal => ContactCategory.PartnerSchoolPrincipal,
                    SchoolContactRole.Coordinator => ContactCategory.PartnerSchoolACC,
                    SchoolContactRole.SciencePrac => ContactCategory.PartnerSchoolSPT,
                    _ => ContactCategory.PartnerSchoolOtherStaff
                };

                result.Add(new ContactResponse(
                    student.StudentId,
                    studentName,
                    student.CurrentGrade,
                    student.School.Name,
                    category,
                    contactName.Value.DisplayName,
                    contactEmail.Value,
                    contactPhone.IsSuccess ? contactPhone.Value : schoolPhone.Value));
            }
        }

        List<Family> families = await _familyRepository.GetFamiliesByStudentId(student.StudentId, cancellationToken);

        foreach (Family family in families)
        {
            Result<EmailAddress> familyEmail = EmailAddress.Create(family.FamilyEmail);

            if (familyEmail.IsFailure)
                continue;

            bool isResidential = family.Students.First(entry => entry.StudentId == student.StudentId).IsResidentialFamily;

            if (isResidential)
            {
                result.Add(new ContactResponse(
                    student.StudentId,
                    studentName,
                    student.CurrentGrade,
                    student.School.Name,
                    ContactCategory.ResidentialFamily,
                    family.FamilyTitle,
                    familyEmail.Value,
                    null));

                foreach (Parent parent in family.Parents)
                {
                    Result<Name> parentName = Name.Create(parent.FirstName, null, parent.LastName);

                    if (parentName.IsFailure)
                        continue;

                    Result<EmailAddress> parentEmail = EmailAddress.Create(parent.EmailAddress);

                    if (parentEmail.IsFailure)
                        continue;

                    Result<PhoneNumber> parentPhone = PhoneNumber.Create(parent.MobileNumber);

                    ContactCategory category = parent.SentralLink switch
                    {
                        Parent.SentralReference.Father => ContactCategory.ResidentialFather,
                        Parent.SentralReference.Mother => ContactCategory.ResidentialMother,
                        _ => ContactCategory.ResidentialFamily
                    };

                    result.Add(new ContactResponse(
                        student.StudentId,
                        studentName,
                        student.CurrentGrade,
                        student.School.Name,
                        category,
                        parentName.Value.DisplayName,
                    parentEmail.Value,
                        parentPhone.IsSuccess ? parentPhone.Value : null));
                }
            }
            else
            {
                result.Add(new ContactResponse(
                    student.StudentId,
                    studentName,
                    student.CurrentGrade,
                    student.School.Name,
                    ContactCategory.NonResidentialFamily,
                    family.FamilyTitle,
                    familyEmail.Value,
                    null));

                foreach (Parent parent in family.Parents)
                {
                    Result<Name> parentName = Name.Create(parent.FirstName, null, parent.LastName);

                    if (parentName.IsFailure)
                        continue;

                    Result<EmailAddress> parentEmail = EmailAddress.Create(parent.EmailAddress);

                    if (parentEmail.IsFailure)
                        continue;

                    Result<PhoneNumber> parentPhone = PhoneNumber.Create(parent.MobileNumber);

                    result.Add(new ContactResponse(
                        student.StudentId,
                        studentName,
                        student.CurrentGrade,
                        student.School.Name,
                        ContactCategory.NonResidentialParent,
                        parentName.Value.DisplayName,
                        parentEmail.Value,
                        parentPhone.IsSuccess ? parentPhone.Value : null));
                }
            }
        }

        List<Offering> studentOfferings = await _offeringRepository.GetByStudentId(student.StudentId, cancellationToken);

        foreach (Offering offering in studentOfferings)
        {
            List<string> staffIds = offering
                .Teachers
                .Where(teacher =>
                    !teacher.IsDeleted &&
                    teacher.Type == AssignmentType.ClassroomTeacher)
                .Select(entry => entry.StaffId)
                .ToList();

            List<Staff> teachers = staffMembers.Where(entry => staffIds.Contains(entry.StaffId)).ToList();

            foreach (Staff teacher in teachers)
            {
                string teacherName = teacher.GetName().DisplayName;
                teacherName += $" ({offering.Name})";

                Result<EmailAddress> teacherEmail = EmailAddress.Create(teacher.EmailAddress);

                if (teacherEmail.IsFailure)
                    continue;

                result.Add(new ContactResponse(
                    student.StudentId,
                    studentName,
                    student.CurrentGrade,
                    student.School.Name,
                    ContactCategory.AuroraTeacher,
                    teacherName,
                    teacherEmail.Value,
                    null));
            }

            Course course = courses.First(entry => entry.Id == offering.CourseId);

            Faculty faculty = faculties.First(entry => entry.Id == course.FacultyId);

            List<string> headTeacherIds = faculty
                .Members
                .Where(member =>
                    !member.IsDeleted &&
                    member.Role == FacultyMembershipRole.Manager)
                .Select(member => member.StaffId)
                .ToList();

            teachers = staffMembers
                .Where(entry => headTeacherIds.Contains(entry.StaffId))
                .ToList();

            foreach (Staff headTeacher in teachers)
            {
                string teacherName = headTeacher.GetName().DisplayName;
                teacherName += $" ({faculty.Name})";

                Result<EmailAddress> teacherEmail = EmailAddress.Create(headTeacher.EmailAddress);

                if (teacherEmail.IsFailure)
                    continue;

                bool existingEntry = result.Any(entry =>
                    entry.Category.Equals(ContactCategory.AuroraHeadTeacher) &&
                entry.Contact == teacherName &&
                    entry.StudentId == student.StudentId);

                if (existingEntry)
                    continue;

                result.Add(new ContactResponse(
                    student.StudentId,
                    studentName,
                    student.CurrentGrade,
                    student.School.Name,
                    ContactCategory.AuroraHeadTeacher,
                    teacherName,
                    teacherEmail.Value,
                    null));
            }
        }

        return result;
    }
}
