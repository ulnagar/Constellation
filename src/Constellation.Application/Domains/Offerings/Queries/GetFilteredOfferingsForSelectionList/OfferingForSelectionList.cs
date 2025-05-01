namespace Constellation.Application.Domains.Offerings.Queries.GetFilteredOfferingsForSelectionList;

using Constellation.Core.Models.Offerings.Identifiers;

public sealed record OfferingForSelectionList(
    OfferingId Id,
    string Name);
