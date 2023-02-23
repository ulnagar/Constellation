﻿namespace Constellation.Application.ClassCovers.GetCoversSummaryByDateAndOffering;

using Constellation.Application.Abstractions.Messaging;
using System;
using System.Collections.Generic;

public sealed record GetCoversSummaryByDateAndOfferingQuery(
    DateOnly CoverDate,
    int OfferingId)
    : IQuery<List<CoverSummaryByDateAndOfferingResponse>>;
