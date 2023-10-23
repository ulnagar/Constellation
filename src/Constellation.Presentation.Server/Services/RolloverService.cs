namespace Constellation.Presentation.Server.Services;

using Core.Shared;
using Shared.Models;

public class RolloverService
{
    private List<RolloverDecision> RolloverDecisions { get; set; } = new();

    public Result RegisterDecision(RolloverDecision rolloverDecision)
    {
        if (string.IsNullOrWhiteSpace(rolloverDecision.StudentId))
            return Result.Failure(new(
                "RolloverService.AddDecision.StudentIdEmpty",
                "Cannot register a Decision without a student Id"));

        if (RolloverDecisions.Any(entry => entry.StudentId == rolloverDecision.StudentId))
            return Result.Failure(new("RolloverService.AddDecision.AlreadyExists",
                $"A Decision has already been registered for student with Id {rolloverDecision.StudentId}"));

        RolloverDecisions.Add(rolloverDecision);

        return Result.Success();
    }

    public void Reset() => RolloverDecisions = new();

    public List<RolloverDecision> GetRegisteredDecisions() => RolloverDecisions;
}
