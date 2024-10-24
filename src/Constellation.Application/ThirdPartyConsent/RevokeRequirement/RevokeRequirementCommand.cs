namespace Constellation.Application.ThirdPartyConsent.RevokeRequirement;

using Abstractions.Messaging;
using Core.Models.ThirdPartyConsent.Identifiers;
using ApplicationId = Core.Models.ThirdPartyConsent.Identifiers.ApplicationId;

public sealed record RevokeRequirementCommand(
    ApplicationId ApplicationId,
    ConsentRequirementId RequirementId)
    : ICommand;