namespace Constellation.Application.ThirdPartyConsent.GetConsentDetails;

using Abstractions.Messaging;
using Core.Models.ThirdPartyConsent.Identifiers;

public sealed record GetConsentDetailsQuery(
    ApplicationId ApplicationId,
    ConsentId ConsentId)
    : IQuery<ConsentDetailsResponse>;
