namespace Constellation.Application.Offerings.GetOfferingsForSelectionList;

using Constellation.Core.Models.Subjects.Identifiers;

public sealed record OfferingSelectionListResponse(
    OfferingId Id,
    string Name);