namespace Constellation.Application.Families.GetFamilyContactsForStudent;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Families.Models;
using Constellation.Core.Abstractions;
using Constellation.Core.Errors;
using Constellation.Core.Models.Families;
using Constellation.Core.Shared;
using Constellation.Core.ValueObjects;
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

        var families = await _familyRepository.GetFamiliesByStudentId(request.StudentId, cancellationToken);

        if (families is null || !families.Any())
        {
            return Result.Failure<List<FamilyContactResponse>>(DomainErrors.Families.Students.NoLinkedFamilies);
        }

        foreach (var family in families)
        {
            var isResidentialFamily = family.Students.Any(student => student.StudentId == request.StudentId && student.IsResidentialFamily);

            var familyEmail = EmailAddress.Create(family.FamilyEmail);

            contacts.Add(new(
                isResidentialFamily,
                Parent.SentralReference.Other,
                family.FamilyTitle,
                familyEmail.Value,
                null,
                null,
                family.Id,
                null));

            foreach (var parent in family.Parents)
            {
                var parentEmail = EmailAddress.Create(parent.EmailAddress);
                var parentMobile = PhoneNumber.Create(parent.MobileNumber);

                contacts.Add(new(
                    isResidentialFamily,
                    parent.SentralLink,
                    $"{parent.FirstName} {parent.LastName}",
                    parentEmail.Value,
                    parentMobile.Value,
                    parent.Id,
                    family.Id,
                    null));
            }
        }

        return contacts;
    }
}
