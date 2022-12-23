namespace Constellation.Application.GroupTutorials.RemoveStudentFromTutorialRoll;

using Constellation.Application.Abstractions.Messaging;
using System;

public sealed record RemoveStudentFromTutorialRollCommand(
    Guid TutorialId,
    Guid RollId,
    string StudentId) : ICommand;
