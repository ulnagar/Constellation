namespace Constellation.Core.DomainEvents;

using Constellation.Core.Models.Identifiers;

public sealed record SciencePracRollSubmittedDomainEvent(
    DomainEventId Id,
    SciencePracLessonId LessonId,
    SciencePracRollId RollId)
    : DomainEvent(Id);