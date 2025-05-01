namespace Constellation.Application.Domains.ThirdPartyConsent.Commands.CreateRequirement;

using Abstractions.Messaging;
using Core.Enums;
using ApplicationId = Core.Models.ThirdPartyConsent.Identifiers.ApplicationId;

public sealed record CreateGradeRequirementCommand(
    ApplicationId ApplicationId,
    Grade Grade)
    : ICommand;