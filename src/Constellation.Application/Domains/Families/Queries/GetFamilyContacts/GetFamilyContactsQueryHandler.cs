namespace Constellation.Application.Domains.Families.Queries.GetFamilyContacts;

using Abstractions.Messaging;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models.Families;
using Constellation.Core.Models.Students.Repositories;
using Core.Extensions;
using Core.Models.Students;
using Core.Models.Students.Identifiers;
using Core.Shared;
using Core.ValueObjects;
using GetFamilyContactsForStudent;
using Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetFamilyContactsQueryHandler
    : IQueryHandler<GetFamilyContactsQuery, List<FamilyContactResponse>>
{
    private readonly IFamilyRepository _familyRepository;
    private readonly IStudentRepository _studentRepository;

    public GetFamilyContactsQueryHandler(
        IFamilyRepository familyRepository,
        IStudentRepository studentRepository)
    {
        _familyRepository = familyRepository;
        _studentRepository = studentRepository;
    }

    public async Task<Result<List<FamilyContactResponse>>> Handle(GetFamilyContactsQuery request, CancellationToken cancellationToken)
    {
        List<FamilyContactResponse> contacts = new();

        List<Family> families = await _familyRepository.GetAllCurrent(cancellationToken);

        foreach (Family family in families)
        {
            bool isResidentialFamily = family.Students.Any(student => student.IsResidentialFamily);

            Result<EmailAddress> familyEmail = EmailAddress.Create(family.FamilyEmail);

            List<StudentId> studentIds = family.Students.Select(member => member.StudentId).ToList();

            List<Student> students = await _studentRepository.GetListFromIds(studentIds, cancellationToken);

            List<string> studentNames = students.OrderBy(student => student.CurrentEnrolment?.Grade).Select(student => $"{student.Name.DisplayName} (Y{student.CurrentEnrolment?.Grade.AsNumber()})").ToList();

            contacts.Add(new(
                isResidentialFamily,
                Parent.SentralReference.Other,
                family.FamilyTitle,
                familyEmail.IsSuccess ? familyEmail.Value : null,
                null,
                null,
                family.Id,
                studentNames));

            foreach (Parent parent in family.Parents)
            {
                Result<EmailAddress> parentEmail = EmailAddress.Create(parent.EmailAddress);
                Result<PhoneNumber> parentMobile = PhoneNumber.Create(parent.MobileNumber);
                Result<Name> name = Name.Create(parent.FirstName, null, parent.LastName);

                contacts.Add(new(
                    isResidentialFamily,
                    parent.SentralLink,
                    name.IsSuccess ? name.Value.DisplayName : string.Empty,
                    parentEmail.IsSuccess ? parentEmail.Value : null,
                    parentMobile.IsSuccess ? parentMobile.Value : null,
                    parent.Id,
                    family.Id,
                    studentNames));
            }
        }

        return contacts;
    }
}
