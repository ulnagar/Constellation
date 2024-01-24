namespace Constellation.Application.ThirdPartyConsent.GetTransactionDetails;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.ThirdPartyConsent.Identifiers;

public sealed record GetTransactionDetailsQuery(
    ConsentTransactionId TransactionId)
    : IQuery<TransactionDetailsResponse>;