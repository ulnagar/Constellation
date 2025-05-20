namespace Constellation.Core.Models.Edval.Events;

using Constellation.Core.IntegrationEvents;
using Models.Identifiers;

public sealed record EdvalTimetablesUpdatedIntegrationEvent(
    IntegrationEventId Id)
    : IntegrationEvent(Id);