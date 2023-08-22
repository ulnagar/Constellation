namespace Constellation.Application.Offerings.GetFilteredOfferingsForSelectionList;

using Constellation.Core.Models.Subjects.Identifiers;

public sealed record OfferingForSelectionList(
    OfferingId Id,
    string Name);
