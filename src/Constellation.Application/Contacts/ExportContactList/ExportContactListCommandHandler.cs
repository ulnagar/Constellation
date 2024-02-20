namespace Constellation.Application.Contacts.ExportContactList;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Contacts.GetContactList;
using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models;
using Constellation.Core.Models.Faculty.ValueObjects;
using Constellation.Core.Models.Families;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Offerings.ValueObjects;
using Constellation.Core.Models.SchoolContacts;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Subjects;
using Constellation.Core.Shared;
using Constellation.Core.ValueObjects;
using Core.Models.Faculty;
using Core.Models.Faculty.Repositories;
using Core.Models.Offerings;
using Core.Models.Offerings.Repositories;
using Helpers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class ExportContactListCommandHandler
    : ICommandHandler<ExportContactListCommand, FileDto>
{
    private readonly IStudentRepository _studentRepository;
    private readonly IFamilyRepository _familyRepository;
    private readonly ISchoolContactRepository _contactRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IFacultyRepository _facultyRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IExcelService _excelService;

    public ExportContactListCommandHandler(
        IStudentRepository studentRepository,
        IFamilyRepository familyRepository,
        ISchoolContactRepository contactRepository,
        IOfferingRepository offeringRepository,
        IStaffRepository staffRepository,
        IFacultyRepository facultyRepository,
        ICourseRepository courseRepository,
        IExcelService excelService)
    {
        _studentRepository = studentRepository;
        _familyRepository = familyRepository;
        _contactRepository = contactRepository;
        _offeringRepository = offeringRepository;
        _staffRepository = staffRepository;
        _facultyRepository = facultyRepository;
        _courseRepository = courseRepository;
        _excelService = excelService;
    }

    public async Task<Result<FileDto>> Handle(ExportContactListCommand request, CancellationToken cancellationToken)
    {
        List<ContactResponse> result = new();

        List<Student> students = await _studentRepository
            .GetFilteredStudents(
                request.OfferingCodes,
                request.Grades,
                request.SchoolCodes,
                cancellationToken);

        List<Offering> offerings = await _offeringRepository
            .GetAll(cancellationToken);

        List<Staff> staffMembers = await _staffRepository
            .GetAll(cancellationToken);

        List<Faculty> faculties = await _facultyRepository
            .GetAll(cancellationToken);

        List<Course> courses = await _courseRepository
            .GetAll(cancellationToken);

        foreach (Student student in students)
        {
            Result<Name> studentName = Name.Create(student.FirstName, null, student.LastName);

            if (studentName.IsFailure)
            {
                // Dunno what to do here!
            }

            Result<EmailAddress> studentEmail = EmailAddress.Create(student.EmailAddress);

            Result<EmailAddress> schoolEmail = EmailAddress.Create(student.School.EmailAddress);

            if (schoolEmail.IsFailure)
            {
                // Dunno what to do here either!
            }

            result.Add(new ContactResponse(
                student.StudentId,
                studentName.Value,
                student.CurrentGrade,
                student.School.Name,
                ContactCategory.Student,
                studentName.Value.DisplayName,
                studentEmail.Value,
                null));

            Result<PhoneNumber> schoolPhone = PhoneNumber.Create(student.School.PhoneNumber);

            result.Add(new ContactResponse(
                student.StudentId,
                studentName.Value,
                student.CurrentGrade,
                student.School.Name,
                ContactCategory.PartnerSchoolSchool,
                student.School.Name,
                schoolEmail.Value,
                schoolPhone.IsSuccess ? schoolPhone.Value : null));

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

                foreach (SchoolContactRole role in contact.Assignments)
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
                        studentName.Value,
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
                        studentName.Value,
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
                            studentName.Value,
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
                        studentName.Value,
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
                            studentName.Value,
                            student.CurrentGrade,
                            student.School.Name,
                            ContactCategory.NonResidentialParent,
                            parentName.Value.DisplayName,
                            parentEmail.Value,
                            parentPhone.IsSuccess ? parentPhone.Value : null));
                    }
                }
            }

            List<OfferingId> offeringIds = student
                .Enrolments
                .Where(entry => !entry.IsDeleted)
                .Select(entry => entry.OfferingId)
                .ToList();

            List<Offering> studentOfferings = offerings.Where(entry => offeringIds.Contains(entry.Id)).ToList();

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
                        studentName.Value,
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
                        studentName.Value,
                        student.CurrentGrade,
                        student.School.Name,
                        ContactCategory.AuroraHeadTeacher,
                        teacherName,
                        teacherEmail.Value,
                        null));
                }
            }
        }

        if (request.ContactCateogries.Any())
        {
            result = result
                .Where(entry =>
                    request.ContactCateogries.Contains(entry.Category))
                .ToList();
        }

        MemoryStream stream = await _excelService.CreateContactExportFile(result, cancellationToken);

        FileDto file = new FileDto
        {
            FileData = stream.ToArray(),
            FileName = "Contacts List.xlsx",
            FileType = FileContentTypes.ExcelModernFile
        };

        return file;
    }
}
