namespace Constellation.Application.Teams.DeleteTeam;

using Constellation.Application.Abstractions.Messaging;
using System;

public sealed record DeleteTeamCommand(Guid Id)
    : ICommand;