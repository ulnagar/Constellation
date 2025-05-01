namespace Constellation.Application.Domains.GroupTutorials.Commands.EditGroupTutorial;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Identifiers;
using System;

public sealed record EditGroupTutorialCommand(
    GroupTutorialId Id,
    string Name,
    DateOnly StartDate,
    DateOnly EndDate) : ICommand;
