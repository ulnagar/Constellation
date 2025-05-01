namespace Constellation.Application.Domains.Rollover.Repositories;

using Core.Models.Rollover;
using Core.Models.Rollover.Errors;
using Core.Models.Rollover.Repositories;
using Core.Models.Students.Identifiers;
using Core.Shared;
using System.Collections.Generic;
using System.Linq;

internal sealed class RolloverRepository : IRolloverRepository
{
    private List<RolloverDecision> RolloverDecisions { get; set; } = new();

    public Result RegisterDecision(RolloverDecision rolloverDecision)
    {
        if (rolloverDecision.StudentId == StudentId.Empty)
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
