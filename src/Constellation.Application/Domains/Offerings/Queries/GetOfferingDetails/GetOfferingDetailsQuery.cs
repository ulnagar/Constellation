namespace Constellation.Application.Domains.Offerings.Queries.GetOfferingDetails;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Offerings.Identifiers;

public sealed record GetOfferingDetailsQuery(
    OfferingId Id)
    : IQuery<OfferingDetailsResponse>;