namespace Constellation.Application.ThirdPartyConsent.GetApplicationsWithoutRequiredConsent;

using Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetApplicationsWithoutRequiredConsentQuery()
    : IQuery<List<ApprovedApplicationResponse>>;