﻿namespace Constellation.Application.Domains.Casuals.Queries.GetCasualsForSelectionList;

using Abstractions.Messaging;
using Core.Abstractions.Repositories;
using Core.Shared;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetCasualsForSelectionListQueryHandler
    : IQueryHandler<GetCasualsForSelectionListQuery, List<CasualsSelectionListResponse>>
{
    private readonly ICasualRepository _casualRepository;

    public GetCasualsForSelectionListQueryHandler(
        ICasualRepository casualRepository)
    {
        _casualRepository = casualRepository;
    }
    public async Task<Result<List<CasualsSelectionListResponse>>> Handle(GetCasualsForSelectionListQuery request, CancellationToken cancellationToken)
    {
        List<CasualsSelectionListResponse> returnData = new();

        var casuals = await _casualRepository.GetAllActive(cancellationToken);

        if (casuals.Count == 0)
            return returnData;

        foreach (var casual in casuals)
        {
            returnData.Add(new CasualsSelectionListResponse(
                casual.Id,
                casual.Name));
        }

        return returnData;
    }
}
