namespace Constellation.Application.ThirdPartyConsent.GetApplications;

using Constellation.Application.Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetApplicationsQuery()
    :IQuery<List<ApplicationSummaryResponse>>;