namespace Constellation.Application.Families.GetFamilyContactsForStudent;

using Abstractions.Messaging;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models.Families;
using Core.Errors;
using Core.Shared;
using Core.ValueObjects;
using Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetFamilyContactsForStudentQueryHandler
    : IQueryHandler<GetFamilyContactsForStudentQuery, List<FamilyContactResponse>>
{
    private readonly IFamilyRepository _familyRepository;

    public GetFamilyContactsForStudentQueryHandler(
        IFamilyRepository familyRepository)
    {
        _familyRepository = familyRepository;
    }

    public async Task<Result<List<FamilyContactResponse>>> Handle(GetFamilyContactsForStudentQuery request, CancellationToken cancellationToken)
    {
        List<FamilyContactResponse> contacts = new();

        List<Family> families = await _familyRepository.GetFamiliesByStudentId(request.StudentId, cancellationToken);

        if (families is null || !families.Any())
        {
            return Result.Failure<List<FamilyContactResponse>>(DomainErrors.Families.Students.NoLinkedFamilies);
        }

        foreach (Family family in families)
        {
            bool isResidentialFamily = family.Students.Any(student => student.StudentId == request.StudentId && student.IsResidentialFamily);

            Result<EmailAddress> familyEmail = EmailAddress.Create(family.FamilyEmail);

            if (familyEmail.IsFailure)
            {
                familyEmail = Result.Success(EmailAddress.None);
            }

            contacts.Add(new(
                isResidentialFamily,
                Parent.SentralReference.Other,
                family.FamilyTitle,
                familyEmail.Value,
                null,
                null,
                family.Id,
                new()));

            foreach (Parent parent in family.Parents)
            {
                Result<EmailAddress> parentEmail = EmailAddress.Create(parent.EmailAddress);
                Result<PhoneNumber> parentMobile = PhoneNumber.Create(parent.MobileNumber);

                contacts.Add(new(
                    isResidentialFamily,
                    parent.SentralLink,
                    $"{parent.FirstName} {parent.LastName}",
                    parentEmail.Value,
                    (parentMobile.IsSuccess ? parentMobile.Value : null),
                    parent.Id,
                    family.Id,
                    new()));
            }
        }

        return contacts;
    }
}
