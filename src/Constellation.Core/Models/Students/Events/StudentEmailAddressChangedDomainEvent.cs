namespace Constellation.Core.Models.Students.Events;

using Constellation.Core.Models.Identifiers;
using DomainEvents;
using Identifiers;

public sealed record StudentEmailAddressChangedDomainEvent(
    DomainEventId Id,
    StudentId StudentId,
    string OldAddress,
    string NewAddress)
    : DomainEvent(Id);