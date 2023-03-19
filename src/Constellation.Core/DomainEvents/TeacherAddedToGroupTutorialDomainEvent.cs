namespace Constellation.Core.DomainEvents;

using Constellation.Core.Models.Identifiers;

public sealed record TeacherAddedToGroupTutorialDomainEvent(
    DomainEventId Id, 
    GroupTutorialId TutorialId, 
    TutorialTeacherId TutorialTeacherId) 
    : DomainEvent(Id);
