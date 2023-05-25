namespace Constellation.Core.DomainEvents;

using Constellation.Core.Models.Identifiers;

public sealed record PendingVerificationResponseCreatedDomainEvent(
    DomainEventId Id,
    AbsenceResponseId ResponseId,
    AbsenceId AbsenceId)
    : DomainEvent(Id);
