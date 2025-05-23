﻿namespace Constellation.Application.Domains.ThirdPartyConsent.Commands.DisableApplication;

using Constellation.Application.Abstractions.Messaging;
using Core.Models.ThirdPartyConsent.Identifiers;

public sealed record DisableApplicationCommand(
    ApplicationId ApplicationId)
    : ICommand;