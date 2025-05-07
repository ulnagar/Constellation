namespace Constellation.Core.Models.Edval.Events;

using Constellation.Core.IntegrationEvents;
using Identifiers;

public sealed record EdvalClassMembershipsUpdatedIntegrationEvent(
    IntegrationEventId Id)
    : IntegrationEvent(Id);