namespace Constellation.Application.Casuals.DeleteCasual;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Identifiers;
using System;

public sealed record DeleteCasualCommand(
    CasualId CasualId)
    : ICommand;