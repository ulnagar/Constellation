namespace Constellation.Application.Casuals.DeleteCasual;

using Constellation.Application.Abstractions.Messaging;
using System;

public sealed record DeleteCasualCommand(
    Guid CasualId)
    : ICommand;