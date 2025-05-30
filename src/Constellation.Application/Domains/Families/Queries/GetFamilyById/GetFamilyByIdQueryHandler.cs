﻿namespace Constellation.Application.Domains.Families.Queries.GetFamilyById;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Domains.Families.Models;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Shared;
using Constellation.Core.ValueObjects;
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
        var family = await _familyRepository.GetFamilyById(request.FamilyId, cancellationToken);

        if (family is null)
        {
            return Result.Failure<FamilyResponse>(FamilyErrors.NotFound(request.FamilyId));
        }

        List<ParentResponse> parents = new();

        foreach (var parent in family.Parents)
        {
            var name = Name.Create(parent.FirstName, null, parent.LastName);

            if (name.IsFailure)
                continue;

            parents.Add(new(parent.Id, name.Value.DisplayName));
        }

        return new FamilyResponse(
            family.Id,
            family.FamilyTitle,
            parents);
    }
}
