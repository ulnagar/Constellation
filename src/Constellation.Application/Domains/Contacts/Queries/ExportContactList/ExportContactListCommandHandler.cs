namespace Constellation.Application.Domains.Contacts.Queries.ExportContactList;

using Abstractions.Messaging;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Constellation.Core.Models.Students.Identifiers;
using Core.Abstractions.Repositories;
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
using Core.Models.Students.Repositories;
using Core.Models.Subjects;
using Core.Models.Subjects.Repositories;
using Core.Shared;
using Core.ValueObjects;
using DTOs;
using Helpers;
using Interfaces;
using Models;
using SchoolContacts.Helpers;
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
    private readonly ISchoolRepository _schoolRepository;
    private readonly IStudentFlagCacheService _flagCache;
    private readonly IExcelService _excelService;

    public ExportContactListCommandHandler(
        IStudentRepository studentRepository,
        IFamilyRepository familyRepository,
        ISchoolContactRepository contactRepository,
        IOfferingRepository offeringRepository,
        IStaffRepository staffRepository,
        IFacultyRepository facultyRepository,
        ICourseRepository courseRepository,
        ISchoolRepository schoolRepository,
        IStudentFlagCacheService flagCache,
        IExcelService excelService)
    {
        _studentRepository = studentRepository;
        _familyRepository = familyRepository;
        _contactRepository = contactRepository;
        _offeringRepository = offeringRepository;
        _staffRepository = staffRepository;
        _facultyRepository = facultyRepository;
        _courseRepository = courseRepository;
        _schoolRepository = schoolRepository;
        _flagCache = flagCache;
        _excelService = excelService;
    }

    public async Task<Result<FileDto>> Handle(ExportContactListCommand request, CancellationToken cancellationToken)
    {
        List<ContactResponse> result = [];

        List<Student> students = await _studentRepository
            .GetFilteredStudents(
                request.Filter.OfferingIds,
                request.Filter.CourseIds,
                request.Filter.Grades,
                request.Filter.SchoolCodes,
                cancellationToken);

        if (request.Filter.Flags.Count > 0)
        {
            List<StudentId> studentIds = [];

            foreach (StudentFlag flag in request.Filter.Flags)
            {
                List<StudentId> idsWithFlag = await _flagCache.GetStudentsWithFlag(flag);
                studentIds.AddRange(idsWithFlag);

                studentIds = studentIds
                    .Distinct()
                    .ToList();
            }

            students = students
                .Where(student => studentIds.Contains(student.Id))
                .ToList();
        }

        List<StaffMember> staffMembers = await _staffRepository
            .GetAll(cancellationToken);

        List<Faculty> faculties = await _facultyRepository
            .GetAll(cancellationToken);

        List<Course> courses = await _courseRepository
            .GetAll(cancellationToken);

        List<School> schools = await _schoolRepository
            .GetAllActive(cancellationToken);

        foreach (Student student in students)
        {
            SchoolEnrolment? enrolment = student.CurrentEnrolment;

            if (enrolment is null)
                continue;

            result.Add(new(
                student.StudentReferenceNumber,
                student.Name,
                enrolment.Grade,
                enrolment.SchoolName,
                ContactCategory.Student,
                student.Id,
                student.Name.DisplayName,
                student.EmailAddress,
                PhoneNumber.Empty,
                string.Empty));

            School? school = schools.FirstOrDefault(entry => entry.Code == enrolment.SchoolCode);

            if (school is null)
                continue;

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
                    schoolPhone.IsSuccess ? schoolPhone.Value : PhoneNumber.Empty,
                    string.Empty));
            }
            
            List<SchoolContact> contacts = await _contactRepository.GetWithRolesBySchool(enrolment.SchoolCode, cancellationToken);

            foreach (SchoolContact contact in contacts)
            {
                foreach (SchoolContactRole role in contact.Assignments.Where(role => role.SchoolCode == enrolment.SchoolCode))
                {
                    if (role.IsDeleted)
                        continue;

                    // If the request should not include restricted roles, ignore restricted roles.
                    if (!request.IncludeRestrictedRoles && role.IsContactRoleRestricted())
                        continue;

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
                        category == ContactCategory.PartnerSchoolOtherStaff ? role.Role.Name + ": " + role.Note : role.Note));
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
                        PhoneNumber.Empty,
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
                        PhoneNumber.Empty,
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
                        PhoneNumber.Empty,
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
                        PhoneNumber.Empty,
                        string.Empty));
                }
            }
        }

        if (request.Filter.Categories.Count > 0)
        {
            result = result
                .Where(entry =>
                    request.Filter.Categories.Contains(entry.Category))
                .ToList();
        }

        MemoryStream stream = await _excelService.CreateContactExportFile(result, cancellationToken);

        FileDto file = new()
        {
            FileData = stream.ToArray(),
            FileName = "Contacts List.xlsx",
            FileType = FileContentTypes.ExcelModernFile
        };

        return file;
    }
}
