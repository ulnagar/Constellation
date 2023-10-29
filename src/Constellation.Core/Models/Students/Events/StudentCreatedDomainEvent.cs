namespace Constellation.Core.Models.Students.Events;

using DomainEvents;
using Identifiers;

public sealed record StudentCreatedDomainEvent(
    DomainEventId Id,
    string StudentId)
    : DomainEvent(Id);