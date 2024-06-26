﻿namespace Constellation.Application.Offerings.GetOfferingSummary;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Offerings.Models;
using Constellation.Core.Models.Offerings.Identifiers;

public sealed record GetOfferingSummaryQuery(
    OfferingId OfferingId)
    : IQuery<OfferingSummaryResponse>;