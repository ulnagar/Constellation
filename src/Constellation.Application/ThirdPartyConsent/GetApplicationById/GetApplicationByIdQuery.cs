namespace Constellation.Application.ThirdPartyConsent.GetApplicationById;

using Abstractions.Messaging;
using Constellation.Application.ThirdPartyConsent.GetApplications;
using Constellation.Core.Models.ThirdPartyConsent.Identifiers;

public sealed record GetApplicationByIdQuery(
    ApplicationId ApplicationId)
    : IQuery<ApplicationResponse>;