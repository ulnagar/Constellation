namespace Constellation.Application.Domains.Families.Queries.GetFamilyById;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Domains.Families.Models;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Shared;
using Core.Models.Families;
using Core.Models.Families.Errors;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetFamilyByIdQueryHandler
    : IQueryHandler<GetFamilyByIdQuery, FamilyResponse>
{
    private readonly IFamilyRepository _familyRepository;

    public GetFamilyByIdQueryHandler(
        IFamilyRepository familyRepository)
    {
        _familyRepository = familyRepository;
    }

    public async Task<Result<FamilyResponse>> Handle(GetFamilyByIdQuery request, CancellationToken cancellationToken)
    {
        Family? family = await _familyRepository.GetFamilyById(request.FamilyId, cancellationToken);

        if (family is null)
            return Result.Failure<FamilyResponse>(FamilyErrors.NotFound(request.FamilyId));

        List<ParentResponse> parents = new();

        foreach (Parent parent in family.Parents)
            parents.Add(new(parent.Id, parent.Name));

        return new FamilyResponse(
            family.Id,
            family.FamilyTitle,
            parents);
    }
}
