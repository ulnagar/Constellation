﻿namespace Constellation.Application.Awards.CreateAwardNomination;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.ValueObjects;

public sealed record CreateAwardNominationCommand(
    AwardNominationPeriodId PeriodId,
    AwardType AwardType,
    int CourseId,
    OfferingId OfferingId,
    string StudentId)
    : ICommand;