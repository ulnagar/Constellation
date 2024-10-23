namespace Constellation.Core.Models.ThirdPartyConsent.Events;

using DomainEvents;
using Constellation.Core.Models.Identifiers;
using Identifiers;

public sealed record ConsentTransactionReceivedDomainEvent(
    DomainEventId Id,
    ConsentTransactionId TransactionId)
    : DomainEvent(Id);

//TODO: R16.1 -> When a transaction is created, generate a PDF document for this and email to the parent who submitted the transaction, for their records.