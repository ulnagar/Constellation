namespace Constellation.Application.Domains.GroupTutorials.Commands.RemoveStudentFromTutorial;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Identifiers;
using System;

public sealed record RemoveStudentFromTutorialCommand(
    GroupTutorialId TutorialId,
    TutorialEnrolmentId EnrolmentId,
    DateOnly? EffectiveFrom) : ICommand;
