namespace Constellation.Application.Offerings.GetOfferingsForSelectionList;

using Constellation.Application.Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetOfferingsForSelectionListQuery
    : IQuery<List<OfferingSelectionListResponse>>;
