namespace Constellation.Application.Domains.ThirdPartyConsent.Queries.GetConsentStatusByApplication;

using Core.ValueObjects;
using System.Collections.Generic;
using ApplicationId = Core.Models.ThirdPartyConsent.Identifiers.ApplicationId;

public sealed record ConsentStatusResponse(
    ApplicationId ApplicationId,
    string ApplicationName,
    bool ConsentRequired,
    List<Name> ConsentGranted,
    List<Name> ConsentDenied,
    List<Name> ConsentPending);