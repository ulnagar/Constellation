﻿namespace Constellation.Application.ThirdPartyConsent.GetTransactionsWithFilter;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.ThirdPartyConsent.Models;
using Constellation.Core.Enums;
using Constellation.Core.Models.Offerings.Identifiers;
using System.Collections.Generic;

public sealed record GetTransactionsWithFilterQuery(
    List<OfferingId> OfferingCodes,
    List<Grade> Grades,
    List<string> SchoolCodes,
    List<string> StudentIds)
    : IQuery<List<TransactionSummaryResponse>>;