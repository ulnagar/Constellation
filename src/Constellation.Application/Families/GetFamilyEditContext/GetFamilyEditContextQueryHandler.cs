namespace Constellation.Application.Families.GetFamilyEditContext;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Extensions;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions;
using Constellation.Core.Errors;
using Constellation.Core.Shared;
using Constellation.Core.ValueObjects;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetFamilyEditContextQueryHandler
    : IQueryHandler<GetFamilyEditContextQuery, FamilyEditContextResponse>
{
    private readonly IFamilyRepository _familyRepository;
    private readonly IStudentRepository _studentRepository;

    public GetFamilyEditContextQueryHandler(
        IFamilyRepository familyRepository,
        IStudentRepository studentRepository)
    {
        _familyRepository = familyRepository;
        _studentRepository = studentRepository;
    }

    public async Task<Result<FamilyEditContextResponse>> Handle(GetFamilyEditContextQuery request, CancellationToken cancellationToken)
    {
        var family = await _familyRepository.GetFamilyById(request.FamilyId, cancellationToken);

        if (family is null)
            return Result.Failure<FamilyEditContextResponse>(DomainErrors.Families.Family.NotFound(request.FamilyId));

        List<string> parents = new();

        foreach (var parent in family.Parents)
        {
            var name = Name.Create(parent.FirstName, null, parent.LastName);

            if (name.IsSuccess)
                parents.Add(name.Value.DisplayName);
        }

        List<string> students = new();

        foreach (var member in family.Students)
        {
            var student = await _studentRepository.GetById(member.StudentId, cancellationToken);

            if (student is not null)
                students.Add($"{student.DisplayName} ({student.CurrentGrade.AsName()})");
        }

        return new FamilyEditContextResponse(
            family.Id,
            family.FamilyTitle,
            family.AddressLine1,
            family.AddressLine2,
            family.AddressTown,
            family.AddressPostCode,
            family.FamilyEmail,
            students,
            parents);
    }
}
