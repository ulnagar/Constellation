namespace Constellation.Core.DomainEvents;

using System;

public sealed record TeacherAddedToGroupTutorialDomainEvent(Guid Id, Guid TutorialId, string TeacherId) : DomainEvent(Id);
