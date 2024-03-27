namespace Constellation.Application.Awards.CreateNominationPeriod;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Identifiers;
using System;

public sealed record UpdateNominationPeriodCommand(
    AwardNominationPeriodId PeriodId,
    string Name,
    DateOnly LockoutDate)
    : ICommand;
