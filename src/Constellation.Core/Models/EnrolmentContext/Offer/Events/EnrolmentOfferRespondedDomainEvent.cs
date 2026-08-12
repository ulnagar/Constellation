namespace Constellation.Core.Models.EnrolmentContext.Offer.Events;

using DomainEvents;
using Identifiers;
using Models.Identifiers;

public sealed record EnrolmentOfferRespondedDomainEvent(
    DomainEventId Id,
    OfferId OfferId)
    : DomainEvent(Id);