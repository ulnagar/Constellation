﻿namespace Constellation.Application.Rollover.Repositories;

using Constellation.Core.Models.Rollover;
using Constellation.Core.Models.Rollover.Errors;
using Constellation.Core.Models.Rollover.Repositories;
using Constellation.Core.Shared;
using System.Collections.Generic;
using System.Linq;

internal sealed class RolloverRepository : IRolloverRepository
{
    private List<RolloverDecision> RolloverDecisions { get; set; } = new();

    public Result RegisterDecision(RolloverDecision rolloverDecision)
    {
        if (string.IsNullOrWhiteSpace(rolloverDecision.StudentId))
            return Result.Failure(RolloverErrors.StudentIdEmpty);

        if (RolloverDecisions.Any(entry => entry.StudentId == rolloverDecision.StudentId))
            return Result.Failure(RolloverErrors.AlreadyExists(rolloverDecision.StudentId));

        RolloverDecisions.Add(rolloverDecision);

        return Result.Success();
    }

    public void Reset() => RolloverDecisions = new();

    public void Remove(RolloverDecision rolloverDecision) => RolloverDecisions.Remove(rolloverDecision);

    public List<RolloverDecision> GetRegisteredDecisions() => RolloverDecisions;


}
