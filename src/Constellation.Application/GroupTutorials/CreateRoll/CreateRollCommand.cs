namespace Constellation.Application.GroupTutorials.CreateRoll;

using Constellation.Application.Abstractions.Messaging;
using System;

public sealed record CreateRollCommand(
    Guid TutorialId,
    DateOnly RollDate) : ICommand<Guid>;