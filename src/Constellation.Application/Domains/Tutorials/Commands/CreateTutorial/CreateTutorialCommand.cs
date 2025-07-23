namespace Constellation.Application.Domains.Tutorials.Commands.CreateTutorial;

using Abstractions.Messaging;
using Core.Models.Tutorials.Identifiers;
using Core.Models.Tutorials.ValueObjects;
using System;

public sealed record CreateTutorialCommand(
    TutorialName Name,
    DateOnly StartDate,
    DateOnly EndDate)
    : ICommand<TutorialId>;
