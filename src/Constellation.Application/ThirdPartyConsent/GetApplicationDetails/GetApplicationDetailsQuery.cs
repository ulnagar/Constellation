namespace Constellation.Application.ThirdPartyConsent.GetApplicationDetails;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.ThirdPartyConsent.Identifiers;

public sealed record GetApplicationDetailsQuery(
    ApplicationId ApplicationId)
    : IQuery<ApplicationDetailsResponse>;