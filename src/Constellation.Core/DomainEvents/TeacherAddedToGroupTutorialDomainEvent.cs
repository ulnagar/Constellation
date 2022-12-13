namespace Constellation.Core.DomainEvents;

using System;

public sealed record TeacherAddedToGroupTutorialDomainEvent(
    Guid Id, 
    Guid TutorialId, 
    Guid TutorialTeacherId) 
    : DomainEvent(Id);
