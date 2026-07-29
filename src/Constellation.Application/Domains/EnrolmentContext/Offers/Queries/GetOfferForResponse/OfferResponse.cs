namespace Constellation.Application.Domains.EnrolmentContext.Offers.Queries.GetOfferForResponse;

using Constellation.Core.Enums;
using Constellation.Core.Models.EnrolmentContext.EnrolmentPeriod.Enums;
using Constellation.Core.Models.EnrolmentContext.Offer.Identifiers;
using Core.Models.EnrolmentContext.EnrolmentPeriod.Identifiers;
using Core.Models.EnrolmentContext.Offer.Enums;
using Core.ValueObjects;
using System;
using ApplicationId = Core.Models.EnrolmentContext.Application.Identifiers.ApplicationId;

public sealed record OfferResponse(
    OfferId Id,
    ApplicationId ApplicationId,
    EnrolmentPeriodId PeriodId,
    Name Student,
    Grade Grade,
    Program Program,
    OfferStatus Status,
    DateTimeOffset? OfferedAt,
    DateTimeOffset? RespondBy,
    DateTimeOffset? RespondedAt,
    bool HasCourtOrders,
    bool HasHealthConcerns);
