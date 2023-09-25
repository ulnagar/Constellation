namespace Constellation.Application.Enrolments.GetCurrentEnrolmentsForOffering;

using Abstractions.Messaging;
using Core.Models.Offerings.Identifiers;
using System.Collections.Generic;

public sealed record GetCurrentEnrolmentsForOfferingQuery(
    OfferingId OfferingId)
    : IQuery<List<EnrolmentResponse>>;