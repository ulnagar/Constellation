namespace Constellation.Core.Models.Assessments.Events;

using DomainEvents;
using Identifiers;
using Models.Identifiers;

public sealed record AssessmentSubmissionReceivedDomainEvent(
    DomainEventId Id,
    AssessmentId AssessmentId,
    SubmissionId SubmissionId)
    : DomainEvent(Id);
