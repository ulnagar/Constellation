namespace Constellation.Core.DomainEvents;

using Constellation.Core.Models.Identifiers;
using System;

public sealed record MicrosoftTeamArchivedDomainEvent(
    DomainEventId Id,
    Guid TeamId)
    : DomainEvent(Id);
