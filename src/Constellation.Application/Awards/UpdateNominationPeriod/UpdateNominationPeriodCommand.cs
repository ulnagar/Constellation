namespace Constellation.Application.Awards.CreateNominationPeriod;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Enums;
using Constellation.Core.Models.Identifiers;
using System;
using System.Collections.Generic;

public sealed record UpdateNominationPeriodCommand(
    AwardNominationPeriodId PeriodId,
    string Name,
    DateOnly LockoutDate)
    : ICommand;
