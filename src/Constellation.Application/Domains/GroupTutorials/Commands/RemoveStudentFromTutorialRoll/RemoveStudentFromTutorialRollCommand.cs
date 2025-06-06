﻿namespace Constellation.Application.Domains.GroupTutorials.Commands.RemoveStudentFromTutorialRoll;

using Abstractions.Messaging;
using Constellation.Core.Models.Identifiers;
using Core.Models.Students.Identifiers;

public sealed record RemoveStudentFromTutorialRollCommand(
    GroupTutorialId TutorialId,
    TutorialRollId RollId,
    StudentId StudentId) : ICommand;
