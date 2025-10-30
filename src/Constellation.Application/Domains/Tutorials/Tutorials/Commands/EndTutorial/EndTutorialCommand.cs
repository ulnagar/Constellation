namespace Constellation.Application.Domains.Tutorials.Tutorials.Commands.EndTutorial;

using Abstractions.Messaging;
using Core.Models.Tutorials.Identifiers;
using System;

public sealed record EndTutorialCommand(
    TutorialId TutorialId,
    DateOnly EndDate)
    : ICommand;
