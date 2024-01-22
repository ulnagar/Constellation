namespace Constellation.Application.ThirdPartyConsent.ReenableApplication;

using Constellation.Application.Abstractions.Messaging;
using Core.Models.ThirdPartyConsent.Identifiers;

public sealed record ReenableApplicationCommand(
    ApplicationId ApplicationId)
    : ICommand;