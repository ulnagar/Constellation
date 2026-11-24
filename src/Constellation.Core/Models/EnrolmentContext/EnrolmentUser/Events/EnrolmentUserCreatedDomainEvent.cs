namespace Constellation.Core.Models.EnrolmentContext.EnrolmentUser.Events;

using Constellation.Core.DomainEvents;
using Constellation.Core.Models.Identifiers;
using System;

public sealed record EnrolmentUserEmailConfirmedDomainEvent(
    DomainEventId Id,
    Guid UserId)
    : DomainEvent(Id);