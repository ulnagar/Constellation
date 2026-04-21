namespace Constellation.Core.Models.Assessments.Events;

using DomainEvents;
using Identifiers;
using Models.Identifiers;

public sealed record AssessmentCreatedDomainEvent(
    DomainEventId Id,
    AssessmentId AssessmentId)
    : DomainEvent(Id);