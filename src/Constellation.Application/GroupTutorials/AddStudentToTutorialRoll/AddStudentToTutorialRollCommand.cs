namespace Constellation.Application.GroupTutorials.AddStudentToRoll;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Identifiers;

public sealed record AddStudentToTutorialRollCommand(
    GroupTutorialId TutorialId,
    TutorialRollId RollId,
    string StudentId) : ICommand;
