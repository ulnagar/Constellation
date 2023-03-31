namespace Constellation.Core.DomainEvents;

using Constellation.Core.Models.Identifiers;

public sealed record AssignmentAttemptSubmittedDomainEvent(
    DomainEventId Id,
    AssignmentId AssignmentId,
    AssignmentSubmissionId SubmissionId)
    : DomainEvent(Id);