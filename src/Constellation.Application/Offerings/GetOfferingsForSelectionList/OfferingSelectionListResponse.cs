namespace Constellation.Application.Offerings.GetOfferingsForSelectionList;

using Constellation.Core.Models.Offerings.Identifiers;

public sealed record OfferingSelectionListResponse(
    OfferingId Id,
    string Name);