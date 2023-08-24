namespace Constellation.Application.Offerings.GetOfferingDetails;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Subjects.Identifiers;

public sealed record GetOfferingDetailsQuery(
    OfferingId Id)
    : IQuery<OfferingDetailsResponse>;