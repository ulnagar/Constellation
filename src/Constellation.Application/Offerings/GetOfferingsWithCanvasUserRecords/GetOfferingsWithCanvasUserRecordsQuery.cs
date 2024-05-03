namespace Constellation.Application.Offerings.GetOfferingsWithCanvasUserRecords;

using Constellation.Application.Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetOfferingsWithCanvasUserRecordsQuery()
    : IQuery<List<OfferingSummaryWithUsers>>;