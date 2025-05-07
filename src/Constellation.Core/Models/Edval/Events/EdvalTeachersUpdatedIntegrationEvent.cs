namespace Constellation.Core.Models.Edval.Events;

using Constellation.Core.IntegrationEvents;
using Identifiers;

public sealed record EdvalTeachersUpdatedIntegrationEvent(
    IntegrationEventId Id)
    : IntegrationEvent(Id);