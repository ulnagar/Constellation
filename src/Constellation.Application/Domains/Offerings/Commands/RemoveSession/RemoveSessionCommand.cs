﻿namespace Constellation.Application.Domains.Offerings.Commands.RemoveSession;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Offerings.Identifiers;

public sealed record RemoveSessionCommand(
    OfferingId OfferingId,
    SessionId SessionId)
    : ICommand;
