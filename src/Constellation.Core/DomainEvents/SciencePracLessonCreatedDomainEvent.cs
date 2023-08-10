namespace Constellation.Core.DomainEvents;

using Constellation.Core.Models.Identifiers;

public sealed record SciencePracLessonCreatedDomainEvent(
    DomainEventId Id,
    SciencePracLessonId LessonId)
    : DomainEvent(Id);