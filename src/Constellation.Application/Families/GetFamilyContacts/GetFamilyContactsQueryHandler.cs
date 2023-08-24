namespace Constellation.Application.Families.GetFamilyContacts;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Extensions;
using Constellation.Application.Families.GetFamilyContactsForStudent;
using Constellation.Application.Families.Models;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models.Families;
using Constellation.Core.Shared;
using Constellation.Core.ValueObjects;
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

        var families = await _familyRepository.GetAllCurrent(cancellationToken);

        foreach (var family in families)
        {
            var isResidentialFamily = family.Students.Any(student => student.IsResidentialFamily);

            var familyEmail = EmailAddress.Create(family.FamilyEmail);

            var studentIds = family.Students.Select(member => member.StudentId).ToList();

            var students = await _studentRepository.GetListFromIds(studentIds, cancellationToken);

            var studentNames = students.OrderBy(student => student.CurrentGrade).Select(student => $"{student.DisplayName} (Y{student.CurrentGrade.AsNumber()})").ToList();

            contacts.Add(new(
                isResidentialFamily,
                Parent.SentralReference.Other,
                family.FamilyTitle,
                (familyEmail.IsSuccess ? familyEmail.Value : null),
                null,
                null,
                family.Id,
                studentNames));

            foreach (var parent in family.Parents)
            {
                var parentEmail = EmailAddress.Create(parent.EmailAddress);
                var parentMobile = PhoneNumber.Create(parent.MobileNumber);
                var name = Name.Create(parent.FirstName, null, parent.LastName);

                contacts.Add(new(
                    isResidentialFamily,
                    parent.SentralLink,
                    (name.IsSuccess ? name.Value.DisplayName : string.Empty),
                    (parentEmail.IsSuccess ? parentEmail.Value : null),
                    (parentMobile.IsSuccess ? parentMobile.Value : null),
                    parent.Id,
                    family.Id,
                    studentNames));
            }
        }

        return contacts;
    }
}
