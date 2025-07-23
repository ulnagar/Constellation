namespace Constellation.Application.Domains.Tutorials.Commands.UpdateTutorial;

using Abstractions.Messaging;
using Core.Models.Tutorials.Identifiers;
using Core.Models.Tutorials.ValueObjects;
using System;

public sealed record UpdateTutorialCommand(
    TutorialId Id,
    TutorialName Name,
    DateOnly StartDate,
    DateOnly EndDate)
    : ICommand;
