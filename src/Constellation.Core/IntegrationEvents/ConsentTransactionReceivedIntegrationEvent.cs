namespace Constellation.Core.IntegrationEvents;

using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.ThirdPartyConsent.Identifiers;

public sealed record ConsentTransactionReceivedIntegrationEvent(
    IntegrationEventId Id,
    ConsentTransactionId TransactionId)
    : IntegrationEvent(Id);