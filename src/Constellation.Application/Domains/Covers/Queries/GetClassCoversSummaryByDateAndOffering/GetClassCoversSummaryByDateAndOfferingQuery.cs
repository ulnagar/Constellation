namespace Constellation.Application.Domains.Covers.Queries.GetClassCoversSummaryByDateAndOffering;

using Abstractions.Messaging;
using Core.Models.Offerings.Identifiers;
using System;
using System.Collections.Generic;

public sealed record GetClassCoversSummaryByDateAndOfferingQuery(
    DateOnly CoverDate,
    OfferingId OfferingId)
    : IQuery<List<ClassCoverSummaryByDateAndOfferingResponse>>;
