namespace Constellation.Core.DomainEvents;

using System;

public sealed record MicrosoftTeamArchivedDomainEvent(
    Guid Id,
    Guid TeamId)
    : DomainEvent(Id);
