﻿namespace Constellation.Application.Domains.Offerings.Queries.GetFilteredOfferingsForSelectionList;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Models.Subjects.Identifiers;
using Constellation.Core.Shared;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetFilteredOfferingsForSelectionListQueryHandler
    : IQueryHandler<GetFilteredOfferingsForSelectionListQuery, List<OfferingForSelectionList>>
{
    private readonly IOfferingRepository _offeringRepository;
    private readonly ILogger _logger;

    public GetFilteredOfferingsForSelectionListQueryHandler(
        IOfferingRepository offeringRepository,
        ILogger logger)
    {
        _offeringRepository = offeringRepository;
        _logger = logger.ForContext<GetFilteredOfferingsForSelectionListQuery>();
    }

    public async Task<Result<List<OfferingForSelectionList>>> Handle(GetFilteredOfferingsForSelectionListQuery request, CancellationToken cancellationToken)
    {
        List<OfferingForSelectionList> response = new();

        foreach (CourseId courseId in request.CourseIds)
        {
            List<Offering> offerings = await _offeringRepository.GetByCourseId(courseId, cancellationToken);

            offerings = offerings
                .Where(offering => offering.IsCurrent)
                .ToList();

            foreach (Offering offering in offerings)
            {
                if (response.Any(entry => entry.Id == offering.Id))
                    continue;

                response.Add(new(
                    offering.Id,
                    offering.Name));
            }
        }

        return response;
    }
}
