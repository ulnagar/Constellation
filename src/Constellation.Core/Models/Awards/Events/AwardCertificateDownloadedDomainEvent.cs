namespace Constellation.Core.Models.Awards.Events;

using DomainEvents;
using Identifiers;

public sealed record AwardCertificateDownloadedDomainEvent(
    DomainEventId Id,
    StudentAwardId AwardId)
    : DomainEvent(Id);