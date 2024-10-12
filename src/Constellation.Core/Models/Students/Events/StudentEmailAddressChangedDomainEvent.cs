namespace Constellation.Core.Models.Students.Events;

using Constellation.Core.Models.Identifiers;
using Constellation.Core.ValueObjects;
using DomainEvents;
using Identifiers;

public sealed record StudentEmailAddressChangedDomainEvent(
    DomainEventId Id,
    StudentId StudentId,
    EmailAddress OldAddress,
    EmailAddress NewAddress)
    : DomainEvent(Id);