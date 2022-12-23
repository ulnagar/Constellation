namespace Constellation.Application.GroupTutorials.AddStudentToRoll;

using Constellation.Application.Abstractions.Messaging;
using System;

public sealed record AddStudentToTutorialRollCommand(
    Guid TutorialId,
    Guid RollId,
    string StudentId) : ICommand;
