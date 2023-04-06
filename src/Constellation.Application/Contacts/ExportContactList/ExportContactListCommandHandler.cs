namespace Constellation.Application.Contacts.ExportContactList;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Contacts.GetContactList;
using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Abstractions;
using Constellation.Core.Models;
using Constellation.Core.Models.Families;
using Constellation.Core.Shared;
using Constellation.Core.ValueObjects;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class ExportContactListCommandHandler
    : ICommandHandler<ExportContactListCommand, FileDto>
{
    private readonly IStudentRepository _studentRepository;
    private readonly IFamilyRepository _familyRepository;
    private readonly ISchoolContactRepository _contactRepository;
    private readonly IExcelService _excelService;

    public ExportContactListCommandHandler(
        IStudentRepository studentRepository,
        IFamilyRepository familyRepository,
        ISchoolContactRepository contactRepository,
        IExcelService excelService)
    {
        _studentRepository = studentRepository;
        _familyRepository = familyRepository;
        _contactRepository = contactRepository;
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

        foreach (var student in students)
        {
            var studentName = Name.Create(student.FirstName, null, student.LastName);

            if (studentName.IsFailure)
            {
                // Dunno what to do here!
            }

            var studentEmail = EmailAddress.Create(student.EmailAddress);

            var schoolEmail = EmailAddress.Create(student.School.EmailAddress);

            if (schoolEmail.IsFailure)
            {
                // Dunno what to do here either!
            }

            result.Add(new ContactResponse(
                student.StudentId,
                studentName.Value,
                student.CurrentGrade,
                ContactCategory.Student,
                studentName.Value.DisplayName,
                studentEmail.Value,
                null));

            var schoolPhone = PhoneNumber.Create(student.School.PhoneNumber);

            result.Add(new ContactResponse(
                student.StudentId,
                studentName.Value,
                student.CurrentGrade,
                ContactCategory.PartnerSchoolSchool,
                student.School.Name,
                schoolEmail.Value,
                schoolPhone.IsSuccess ? schoolPhone.Value : null));

            var contacts = await _contactRepository.GetWithRolesBySchool(student.SchoolCode, cancellationToken);

            foreach (var contact in contacts)
            {
                var contactName = Name.Create(contact.FirstName, null, contact.LastName);

                if (contactName.IsFailure)
                    continue;

                var contactEmail = EmailAddress.Create(contact.EmailAddress);

                if (contactEmail.IsFailure)
                    continue;

                var contactPhone = PhoneNumber.Create(contact.PhoneNumber);

                foreach (var role in contact.Assignments)
                {
                    var category = role.Role switch
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
                        category,
                        contactName.Value.DisplayName,
                    contactEmail.Value,
                        contactPhone.IsSuccess ? contactPhone.Value : schoolPhone.Value));
                }
            }

            var families = await _familyRepository.GetFamiliesByStudentId(student.StudentId, cancellationToken);

            foreach (var family in families)
            {
                var familyEmail = EmailAddress.Create(family.FamilyEmail);

                if (familyEmail.IsFailure)
                    continue;

                var isResidential = family.Students.Where(entry => entry.StudentId == student.StudentId).First().IsResidentialFamily;

                if (isResidential)
                {
                    result.Add(new ContactResponse(
                        student.StudentId,
                        studentName.Value,
                        student.CurrentGrade,
                        ContactCategory.ResidentialFamily,
                        family.FamilyTitle,
                        familyEmail.Value,
                        null));

                    foreach (var parent in family.Parents)
                    {
                        var parentName = Name.Create(parent.FirstName, null, parent.LastName);

                        if (parentName.IsFailure)
                            continue;

                        var parentEmail = EmailAddress.Create(parent.EmailAddress);

                        if (parentEmail.IsFailure)
                            continue;

                        var parentPhone = PhoneNumber.Create(parent.MobileNumber);

                        var category = parent.SentralLink switch
                        {
                            Parent.SentralReference.Father => ContactCategory.ResidentialFather,
                            Parent.SentralReference.Mother => ContactCategory.ResidentialMother,
                            _ => ContactCategory.ResidentialFamily
                        };

                        result.Add(new ContactResponse(
                            student.StudentId,
                            studentName.Value,
                            student.CurrentGrade,
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
                        ContactCategory.NonResidentialFamily,
                        family.FamilyTitle,
                        familyEmail.Value,
                        null));

                    foreach (var parent in family.Parents)
                    {
                        var parentName = Name.Create(parent.FirstName, null, parent.LastName);

                        if (parentName.IsFailure)
                            continue;

                        var parentEmail = EmailAddress.Create(parent.EmailAddress);

                        if (parentEmail.IsFailure)
                            continue;

                        var parentPhone = PhoneNumber.Create(parent.MobileNumber);

                        result.Add(new ContactResponse(
                            student.StudentId,
                            studentName.Value,
                            student.CurrentGrade,
                            ContactCategory.NonResidentialParent,
                            parentName.Value.DisplayName,
                            parentEmail.Value,
                            parentPhone.IsSuccess ? parentPhone.Value : null));
                    }
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

        var stream = await _excelService.CreateContactExportFile(result, cancellationToken);

        var file = new FileDto
        {
            FileData = stream.ToArray(),
            FileName = "Contacts List.xlsx",
            FileType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
        };

        return file;
    }
}
