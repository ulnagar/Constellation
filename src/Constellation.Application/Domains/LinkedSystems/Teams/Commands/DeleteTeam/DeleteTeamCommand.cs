namespace Constellation.Application.Domains.LinkedSystems.Teams.Commands.DeleteTeam;

using Abstractions.Messaging;
using System;

public sealed record DeleteTeamCommand(Guid Id)
    : ICommand;