namespace Constellation.Core.Models.Edval.Events;

using Constellation.Core.Models.Identifiers;
using IntegrationEvents;

public sealed record EdvalClassesUpdatedIntegrationEvent(
    IntegrationEventId Id)
    : IntegrationEvent(Id);