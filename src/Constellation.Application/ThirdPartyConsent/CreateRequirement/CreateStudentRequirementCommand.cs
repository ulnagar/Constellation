namespace Constellation.Application.ThirdPartyConsent.CreateRequirement;

using Abstractions.Messaging;
using Core.Models.Students.Identifiers;
using ApplicationId = Core.Models.ThirdPartyConsent.Identifiers.ApplicationId;

public sealed record CreateStudentRequirementCommand(
    ApplicationId ApplicationId,
    StudentId StudentId)
    : ICommand;