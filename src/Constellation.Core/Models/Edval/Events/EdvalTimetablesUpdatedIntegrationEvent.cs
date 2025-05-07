namespace Constellation.Core.Models.Edval.Events;

using Constellation.Core.IntegrationEvents;
using Identifiers;

public sealed record EdvalTimetablesUpdatedIntegrationEvent(
    IntegrationEventId Id)
    : IntegrationEvent(Id);