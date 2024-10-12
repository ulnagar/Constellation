namespace Constellation.Application.GroupTutorials.SubmitRoll;

using Abstractions.Messaging;
using Constellation.Core.Models.Identifiers;
using Core.Models.Students.Identifiers;
using System.Collections.Generic;

public sealed record SubmitRollCommand(
    GroupTutorialId TutorialId,
    TutorialRollId RollId,
    string StaffEmail,
    Dictionary<StudentId,bool> StudentPresence) : ICommand;