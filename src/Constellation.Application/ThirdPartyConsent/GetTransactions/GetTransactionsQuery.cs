namespace Constellation.Application.ThirdPartyConsent.GetTransactions;

using Abstractions.Messaging;
using Constellation.Application.ThirdPartyConsent.Models;
using System.Collections.Generic;

public sealed record GetTransactionsQuery()
    : IQuery<List<TransactionSummaryResponse>>;