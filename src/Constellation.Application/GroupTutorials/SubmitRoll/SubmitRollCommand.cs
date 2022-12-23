namespace Constellation.Application.GroupTutorials.SubmitRoll;

using Constellation.Application.Abstractions.Messaging;
using System;
using System.Collections.Generic;

public sealed record SubmitRollCommand(
    Guid TutorialId,
    Guid RollId,
    string StaffEmail,
    Dictionary<string,bool> StudentPresence) : ICommand;