namespace Constellation.Core.DomainEvents;

using System;

public sealed record MicrosoftTeamRegisteredDomainEvent(
    Guid Id,
    Guid TeamId)
    : DomainEvent(Id);
