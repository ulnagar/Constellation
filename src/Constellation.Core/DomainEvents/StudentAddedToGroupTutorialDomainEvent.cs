namespace Constellation.Core.DomainEvents;

using Constellation.Core.Models.Identifiers;
using System;

public sealed record StudentAddedToGroupTutorialDomainEvent(
    DomainEventId Id,
    GroupTutorialId TutorialId,
    TutorialEnrolmentId EnrolmentId)
    : DomainEvent(Id);
