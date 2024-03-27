namespace Constellation.Application.Offerings.GetFilteredOfferingsForSelectionList;

using Constellation.Core.Models.Offerings.Identifiers;

public sealed record OfferingForSelectionList(
    OfferingId Id,
    string Name);
