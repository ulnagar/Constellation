namespace Constellation.Application.Awards.CreateNominationPeriod;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Enums;
using System;
using System.Collections.Generic;

public sealed record CreateNominationPeriodCommand(
    DateOnly LockoutDate,
    List<Grade> Grades)
    : ICommand;
