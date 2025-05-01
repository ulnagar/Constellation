namespace Constellation.Application.Domains.GroupTutorials.Commands.CreateGroupTutorial;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Identifiers;
using System;

public sealed record CreateGroupTutorialCommand(
    string Name,
    DateOnly StartDate,
    DateOnly EndDate)
    : ICommand<GroupTutorialId>;
