namespace Constellation.Application.ClassCovers.GetCoversSummaryByDateAndOffering;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Subjects.Identifiers;
using System;
using System.Collections.Generic;

public sealed record GetCoversSummaryByDateAndOfferingQuery(
    DateOnly CoverDate,
    OfferingId OfferingId)
    : IQuery<List<CoverSummaryByDateAndOfferingResponse>>;
