namespace Constellation.Core.Models.Awards.Events;

using DomainEvents;
using Models.Identifiers;

public sealed record AwardCertificateDownloadedDomainEvent(
    DomainEventId Id,
    StudentAwardId AwardId)
    : DomainEvent(Id);