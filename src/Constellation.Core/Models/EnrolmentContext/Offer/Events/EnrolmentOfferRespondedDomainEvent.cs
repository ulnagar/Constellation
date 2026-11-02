namespace Constellation.Core.Models.EnrolmentContext.Offer.Events;

using DomainEvents;
using EnrolmentParent.Identifiers;
using Identifiers;
using Models.Identifiers;

public sealed record EnrolmentOfferRespondedDomainEvent(
    DomainEventId Id,
    OfferId OfferId,
    EnrolmentParentId ParentId)
    : DomainEvent(Id);