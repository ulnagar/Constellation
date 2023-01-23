namespace Constellation.Application.Teams.ArchiveTeam;

using Constellation.Application.Abstractions.Messaging;
using System;

public sealed record ArchiveTeamCommand(
    Guid Id)
    : ICommand;
