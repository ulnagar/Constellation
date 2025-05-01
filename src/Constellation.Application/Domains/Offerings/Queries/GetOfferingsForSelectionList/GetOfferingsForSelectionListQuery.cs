namespace Constellation.Application.Domains.Offerings.Queries.GetOfferingsForSelectionList;

using Constellation.Application.Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetOfferingsForSelectionListQuery
    : IQuery<List<OfferingSelectionListResponse>>;
