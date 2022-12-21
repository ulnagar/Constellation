namespace Constellation.Application.GroupTutorials.EditGroupTutorial;

using Constellation.Application.Abstractions.Messaging;
using System;

public sealed record EditGroupTutorialCommand(
    Guid Id,
    string Name,
    DateOnly StartDate,
    DateOnly EndDate) : ICommand;
