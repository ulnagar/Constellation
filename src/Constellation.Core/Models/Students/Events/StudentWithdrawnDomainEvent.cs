namespace Constellation.Core.Models.Students.Events;

using DomainEvents;
using Identifiers;

public sealed record StudentWithdrawnDomainEvent(
    DomainEventId Id,
    string StudentId)
    : DomainEvent(Id);