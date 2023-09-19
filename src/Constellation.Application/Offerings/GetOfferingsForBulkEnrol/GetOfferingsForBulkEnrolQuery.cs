namespace Constellation.Application.Offerings.GetOfferingsForBulkEnrol;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Enums;
using System.Collections.Generic;

public sealed record GetOfferingsForBulkEnrolQuery(
    Grade Grade)
    : IQuery<List<BulkEnrolOfferingResponse>>;