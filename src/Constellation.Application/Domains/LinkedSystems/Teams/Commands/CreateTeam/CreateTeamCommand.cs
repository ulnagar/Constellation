namespace Constellation.Application.Domains.LinkedSystems.Teams.Commands.CreateTeam;

using Abstractions.Messaging;
using System;

public sealed record CreateTeamCommand(
    Guid Id,
    string Name,
    string Description,
    string ChannelId) 
    : ICommand;