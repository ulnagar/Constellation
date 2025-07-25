namespace Constellation.Application.Domains.Tutorials.Commands.AddTeamToTutorial;

using Abstractions.Messaging;
using Core.Models.Tutorials.Identifiers;
using System;

public sealed record AddTeamToTutorialCommand(
    TutorialId TutorialId,
    Guid TeamId)
    : ICommand;