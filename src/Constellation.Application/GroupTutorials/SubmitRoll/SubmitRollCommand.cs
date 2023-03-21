namespace Constellation.Application.GroupTutorials.SubmitRoll;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Identifiers;
using System.Collections.Generic;

public sealed record SubmitRollCommand(
    GroupTutorialId TutorialId,
    TutorialRollId RollId,
    string StaffEmail,
    Dictionary<string,bool> StudentPresence) : ICommand;