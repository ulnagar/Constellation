﻿namespace Constellation.Application.Domains.ClassCovers.Queries.GetCoversSummaryByDateAndOffering;

using Abstractions.Messaging;
using Core.Models.Offerings.Identifiers;
using System;
using System.Collections.Generic;

public sealed record GetCoversSummaryByDateAndOfferingQuery(
    DateOnly CoverDate,
    OfferingId OfferingId)
    : IQuery<List<CoverSummaryByDateAndOfferingResponse>>;
