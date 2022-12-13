namespace Constellation.Core.DomainEvents;

using System;

public sealed record TeacherRemovedFromGroupTutorialDomainEvent(
    Guid Id,
    Guid GroupTutorialId,
    Guid TutorialTeacherId)
    : DomainEvent(Id);
