namespace Constellation.Application.Offerings.GetOfferingDetails;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Offerings.Identifiers;

public sealed record GetOfferingDetailsQuery(
    OfferingId Id)
    : IQuery<OfferingDetailsResponse>;