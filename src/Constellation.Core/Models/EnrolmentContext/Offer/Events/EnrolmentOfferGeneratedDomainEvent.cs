namespace Constellation.Core.Models.EnrolmentContext.Offer.Events;

using DomainEvents;
using Identifiers;
using Models.Identifiers;

public sealed record EnrolmentOfferGeneratedDomainEvent(
    DomainEventId Id,
    OfferId OfferId)
    : DomainEvent(Id);