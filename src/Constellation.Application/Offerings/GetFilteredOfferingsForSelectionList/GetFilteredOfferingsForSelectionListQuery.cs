namespace Constellation.Application.Offerings.GetFilteredOfferingsForSelectionList;

using Constellation.Application.Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetFilteredOfferingsForSelectionListQuery(
    List<int> CourseIds)
    : IQuery<List<OfferingForSelectionList>>;