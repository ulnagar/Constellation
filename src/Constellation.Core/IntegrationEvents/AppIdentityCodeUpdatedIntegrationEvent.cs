namespace Constellation.Core.IntegrationEvents;

using Models.Identifiers;

public sealed record AppIdentityCodeUpdatedIntegrationEvent(
    IntegrationEventId Id)
    : IntegrationEvent(Id);