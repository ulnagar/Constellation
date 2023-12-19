namespace Constellation.Core.Models.Awards.Events;

using Constellation.Core.DomainEvents;
using Constellation.Core.Models.Identifiers;

public sealed record AwardCertificateDownloadedDomainEvent(
    DomainEventId Id,
    StudentAwardId AwardId)
    : DomainEvent(Id);