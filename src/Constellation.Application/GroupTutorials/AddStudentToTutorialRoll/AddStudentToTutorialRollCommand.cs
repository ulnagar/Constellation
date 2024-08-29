﻿namespace Constellation.Application.GroupTutorials.AddStudentToTutorialRoll;

using Abstractions.Messaging;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.Students.Identifiers;

public sealed record AddStudentToTutorialRollCommand(
    GroupTutorialId TutorialId,
    TutorialRollId RollId,
    StudentId StudentId) : ICommand;
