namespace Constellation.Application.Domains.LinkedSystems.Teams.Commands.ArchiveTeam;

using Abstractions.Messaging;
using System;

public sealed record ArchiveTeamCommand(
    Guid Id)
    : ICommand;
