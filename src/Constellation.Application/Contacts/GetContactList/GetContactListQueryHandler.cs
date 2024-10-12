﻿namespace Constellation.Application.Contacts.GetContactList;

using Abstractions.Messaging;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models;
using Constellation.Core.Models.Families;
using Constellation.Core.Models.SchoolContacts;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Repositories;
using Core.Models.Faculties;
using Core.Models.Faculties.Repositories;
using Core.Models.Faculties.ValueObjects;
using Core.Models.Offerings;
using Core.Models.Offerings.Repositories;
using Core.Models.Offerings.ValueObjects;
using Core.Models.SchoolContacts.Repositories;
using Core.Models.StaffMembers.Repositories;
using Core.Models.Subjects;
using Core.Models.Subjects.Repositories;
using Core.Shared;
using Core.ValueObjects;
using Interfaces.Repositories;
using Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetContactListQueryHandler
    : IQueryHandler<GetContactListQuery, List<ContactResponse>>
{
    private readonly IStudentRepository _studentRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IFacultyRepository _facultyRepository;
    private readonly IFamilyRepository _familyRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly ISchoolContactRepository _contactRepository;
    private readonly ISchoolRepository _schoolRepository;

    public GetContactListQueryHandler(
        IStudentRepository studentRepository,
        IOfferingRepository offeringRepository,
        ICourseRepository courseRepository,
        IFacultyRepository facultyRepository,
        IFamilyRepository familyRepository,
        IStaffRepository staffRepository,
        ISchoolContactRepository contactRepository,
        ISchoolRepository schoolRepository)
    {
        _studentRepository = studentRepository;
        _offeringRepository = offeringRepository;
        _courseRepository = courseRepository;
        _facultyRepository = facultyRepository;
        _familyRepository = familyRepository;
        _staffRepository = staffRepository;
        _contactRepository = contactRepository;
        _schoolRepository = schoolRepository;
    }

    public async Task<Result<List<ContactResponse>>> Handle(GetContactListQuery request, CancellationToken cancellationToken)
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

        List<School> schools = await _schoolRepository.GetAllActive(cancellationToken);

        foreach (Student student in students)
        {
            SchoolEnrolment? enrolment = student.CurrentEnrolment;

            if (enrolment is null)
                continue;

            result.Add(new ContactResponse(
                student.Id,
                student.Name,
                enrolment.Grade,
                enrolment.SchoolName,
                ContactCategory.Student,
                student.Name.DisplayName,
                student.EmailAddress,
                null,
                null));

            School school = schools.FirstOrDefault(entry => entry.Code == enrolment.SchoolCode);

            if (school is null)
                continue;

            Result<PhoneNumber> schoolPhone = PhoneNumber.Create(school.PhoneNumber);

            Result<EmailAddress> schoolEmail = EmailAddress.Create(school.EmailAddress);

            if (schoolEmail.IsSuccess)
            {
                result.Add(new ContactResponse(
                    student.Id,
                    student.Name,
                    enrolment.Grade,
                    enrolment.SchoolName,
                    ContactCategory.PartnerSchoolSchool,
                    enrolment.SchoolName,
                    schoolEmail.Value,
                    schoolPhone.IsSuccess ? schoolPhone.Value : null,
                    null));
            }

            List<SchoolContact> contacts = await _contactRepository.GetWithRolesBySchool(enrolment.SchoolCode, cancellationToken);

            foreach (SchoolContact contact in contacts)
            {
                Result<Name> contactName = Name.Create(contact.FirstName, null, contact.LastName);

                if (contactName.IsFailure)
                    continue;

                Result<EmailAddress> contactEmail = EmailAddress.Create(contact.EmailAddress);

                if (contactEmail.IsFailure)
                    continue;

                Result<PhoneNumber> contactPhone = PhoneNumber.Create(contact.PhoneNumber);

                foreach (SchoolContactRole role in contact.Assignments.Where(role => role.SchoolCode == enrolment.SchoolCode))
                {
                    ContactCategory category = role.Role switch
                    {
                        SchoolContactRole.Principal => ContactCategory.PartnerSchoolPrincipal,
                        SchoolContactRole.Coordinator => ContactCategory.PartnerSchoolACC,
                        SchoolContactRole.SciencePrac => ContactCategory.PartnerSchoolSPT,
                        _ => ContactCategory.PartnerSchoolOtherStaff
                    };

                    result.Add(new ContactResponse(
                        student.Id,
                        student.Name,
                        enrolment.Grade,
                        enrolment.SchoolName,
                        category,
                        contactName.Value.DisplayName,
                        contactEmail.Value,
                        contactPhone.IsSuccess ? contactPhone.Value : schoolPhone.Value,
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
                    result.Add(new ContactResponse(
                        student.Id,
                        student.Name,
                        enrolment.Grade,
                        enrolment.SchoolName,
                        ContactCategory.ResidentialFamily,
                        family.FamilyTitle,
                        familyEmail.Value,
                        null,
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
                            student.Id,
                            student.Name,
                            enrolment.Grade,
                            enrolment.SchoolName,
                            category,
                            parentName.Value.DisplayName,
                            parentEmail.Value,
                            parentPhone.IsSuccess ? parentPhone.Value : null,
                            null));
                    }
                }
                else
                {
                    result.Add(new ContactResponse(
                        student.Id,
                        student.Name,
                        enrolment.Grade,
                        enrolment.SchoolName,
                        ContactCategory.NonResidentialFamily,
                        family.FamilyTitle,
                        familyEmail.Value,
                        null,
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
                            student.Id,
                            student.Name,
                            enrolment.Grade,
                            enrolment.SchoolName,
                            ContactCategory.NonResidentialParent,
                            parentName.Value.DisplayName,
                            parentEmail.Value,
                            parentPhone.IsSuccess ? parentPhone.Value : null,
                            null));
                    }
                }
            }

            List<Offering> studentOfferings = await _offeringRepository.GetByStudentId(student.Id, cancellationToken);

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
                        student.Id,
                        student.Name,
                        enrolment.Grade,
                        enrolment.SchoolName,
                        ContactCategory.AuroraTeacher,
                        teacherName,
                        teacherEmail.Value,
                        null,
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
                        entry.StudentId == student.Id);

                    if (existingEntry)
                        continue;

                    result.Add(new ContactResponse(
                        student.Id,
                        student.Name,
                        enrolment.Grade,
                        enrolment.SchoolName,
                        ContactCategory.AuroraHeadTeacher,
                        teacherName,
                        teacherEmail.Value,
                        null,
                        null));
                }
            }
        }

        if (request.ContactCategories.Any())
        {
            result = result
                .Where(entry => 
                    request.ContactCategories.Contains(entry.Category))
                .ToList();
        }

        return result;
    }
}
