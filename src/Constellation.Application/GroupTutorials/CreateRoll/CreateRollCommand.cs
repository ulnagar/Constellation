﻿namespace Constellation.Application.GroupTutorials.CreateRoll;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Identifiers;
using System;

public sealed record CreateRollCommand(
    GroupTutorialId TutorialId,
    DateOnly RollDate) : ICommand<TutorialRollId>;