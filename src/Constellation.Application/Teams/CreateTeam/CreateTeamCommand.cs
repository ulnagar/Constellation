namespace Constellation.Application.Teams.CreateTeam;

using Constellation.Application.Abstractions.Messaging;
using System;

public sealed record CreateTeamCommand(
    Guid Id,
    string Name,
    string Description,
    string ChannelId) 
    : ICommand;