namespace Constellation.Core.DomainEvents;

using Constellation.Core.Models.Identifiers;

public sealed record AbsenceResponseConfirmedDomainEvent(
    DomainEventId Id,
    AbsenceResponseId ResponseId,
    AbsenceId AbsenceId)
    : DomainEvent(Id);