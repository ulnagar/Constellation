namespace Constellation.Application.OfferingEnrolments.GetCurrentEnrolmentsForOffering;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Offerings.Identifiers;
using System.Collections.Generic;

public sealed record GetCurrentEnrolmentsForOfferingQuery(
    OfferingId OfferingId)
    : IQuery<List<EnrolmentResponse>>;