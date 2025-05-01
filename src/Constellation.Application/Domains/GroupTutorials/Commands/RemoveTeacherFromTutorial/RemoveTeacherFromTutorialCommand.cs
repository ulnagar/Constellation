namespace Constellation.Application.Domains.GroupTutorials.Commands.RemoveTeacherFromTutorial;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Identifiers;
using System;

public sealed record RemoveTeacherFromTutorialCommand(
    GroupTutorialId TutorialId, 
    TutorialTeacherId TeacherId,
    DateOnly? TakesEffectOn = null) : ICommand;
