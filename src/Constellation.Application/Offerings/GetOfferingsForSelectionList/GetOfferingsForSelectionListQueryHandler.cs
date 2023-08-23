﻿namespace Constellation.Application.Offerings.GetOfferingsForSelectionList;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Shared;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetOfferingsForSelectionListQueryHandler
    : IQueryHandler<GetOfferingsForSelectionListQuery, List<OfferingSelectionListResponse>>
{
    private readonly IOfferingRepository _offeringRepository;

    public GetOfferingsForSelectionListQueryHandler(
        IOfferingRepository offeringRepository)
    {
        _offeringRepository = offeringRepository;
    }
    public async Task<Result<List<OfferingSelectionListResponse>>> Handle(GetOfferingsForSelectionListQuery request, CancellationToken cancellationToken)
    {
        List<OfferingSelectionListResponse> returnData = new();

        var offerings = await _offeringRepository.GetAllActive(cancellationToken);

        if (offerings.Count == 0)
            return returnData;

        foreach (var offering in offerings)
        {
            returnData.Add(new OfferingSelectionListResponse(
                offering.Id,
                offering.Name));
        }

        return returnData;
    }
}
