namespace Constellation.Application.ThirdPartyConsent.GetConsentStatusByApplication;

using Abstractions.Messaging;
using Core.Enums;
using Core.Models.Offerings.Identifiers;
using System.Collections.Generic;
using ApplicationId = Core.Models.ThirdPartyConsent.Identifiers.ApplicationId;

public sealed record GetConsentStatusByApplicationQuery(
    ApplicationId ApplicationId,
    List<OfferingId> OfferingCodes,
    List<Grade> Grades,
    List<string> SchoolCodes)
    : IQuery<List<ConsentStatusResponse>>;