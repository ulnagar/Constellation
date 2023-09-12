namespace Constellation.Application.Offerings.GetOfferingSummary;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Offerings.Models;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Subjects.Identifiers;

public sealed record GetOfferingSummaryQuery(
    OfferingId OfferingId)
    : IQuery<OfferingSummaryResponse>;