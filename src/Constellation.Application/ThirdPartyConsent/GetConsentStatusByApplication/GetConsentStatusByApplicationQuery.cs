namespace Constellation.Application.ThirdPartyConsent.GetConsentStatusByApplication;

using Abstractions.Messaging;
using Core.Enums;
using Core.Models.Offerings.Identifiers;
using System.Collections.Generic;

public sealed record GetConsentStatusByApplicationQuery(
    List<OfferingId> OfferingCodes,
    List<Grade> Grades)
    : IQuery<List<ConsentStatusResponse>>;