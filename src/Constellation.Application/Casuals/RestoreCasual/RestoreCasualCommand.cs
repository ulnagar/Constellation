namespace Constellation.Application.Casuals.RestoreCasual;

using Constellation.Application.Abstractions.Messaging;
using System;

public sealed record RestoreCasualCommand(
    Guid CasualId)
    : ICommand;
