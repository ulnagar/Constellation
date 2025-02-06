namespace Constellation.Application.Families.GetFamilyEditContext;

using Abstractions.Messaging;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models.Students.Repositories;
using Core.Extensions;
using Core.Models.Families;
using Core.Models.Families.Errors;
using Core.Models.Students;
using Core.Shared;
using Core.ValueObjects;
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
        Family family = await _familyRepository.GetFamilyById(request.FamilyId, cancellationToken);

        if (family is null)
            return Result.Failure<FamilyEditContextResponse>(FamilyErrors.NotFound(request.FamilyId));

        List<string> parents = new();

        foreach (Parent parent in family.Parents)
        {
            Result<Name> name = Name.Create(parent.FirstName, null, parent.LastName);

            if (name.IsSuccess)
                parents.Add(name.Value.DisplayName);
        }

        List<string> students = new();

        foreach (StudentFamilyMembership member in family.Students)
        {
            Student student = await _studentRepository.GetById(member.StudentId, cancellationToken);

            if (student is not null)
                students.Add($"{student.Name.DisplayName} ({student.CurrentEnrolment?.Grade.AsName()})");
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
