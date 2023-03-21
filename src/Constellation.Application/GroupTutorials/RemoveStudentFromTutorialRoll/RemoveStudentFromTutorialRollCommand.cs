namespace Constellation.Application.GroupTutorials.RemoveStudentFromTutorialRoll;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Identifiers;
using System;

public sealed record RemoveStudentFromTutorialRollCommand(
    GroupTutorialId TutorialId,
    TutorialRollId RollId,
    string StudentId) : ICommand;
