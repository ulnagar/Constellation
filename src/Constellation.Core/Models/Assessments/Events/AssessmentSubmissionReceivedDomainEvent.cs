namespace Constellation.Core.Models.Assessments.Events;

using DomainEvents;
using Identifiers;
using Models.Identifiers;
using Students.Identifiers;

public sealed record AssessmentSubmissionReceivedDomainEvent(
    DomainEventId Id,
    AssessmentId AssessmentId,
    StudentId StudentId,
    SubmissionId SubmissionId)
    : DomainEvent(Id);