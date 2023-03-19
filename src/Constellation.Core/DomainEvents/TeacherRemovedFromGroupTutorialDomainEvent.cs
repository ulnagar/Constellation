namespace Constellation.Core.DomainEvents;

using Constellation.Core.Models.Identifiers;
using System;

public sealed record TeacherRemovedFromGroupTutorialDomainEvent(
    DomainEventId Id,
    GroupTutorialId GroupTutorialId,
    TutorialTeacherId TutorialTeacherId)
    : DomainEvent(Id);
