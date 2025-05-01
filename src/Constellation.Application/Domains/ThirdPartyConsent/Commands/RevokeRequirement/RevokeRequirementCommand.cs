namespace Constellation.Application.Domains.ThirdPartyConsent.Commands.RevokeRequirement;

using Abstractions.Messaging;
using Core.Models.ThirdPartyConsent.Identifiers;
using ApplicationId = Core.Models.ThirdPartyConsent.Identifiers.ApplicationId;

public sealed record RevokeRequirementCommand(
    ApplicationId ApplicationId,
    ConsentRequirementId RequirementId)
    : ICommand;