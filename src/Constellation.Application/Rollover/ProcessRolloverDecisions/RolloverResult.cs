namespace Constellation.Application.Rollover.ProcessRolloverDecisions;

using Core.Models.Rollover;
using Core.Shared;

public sealed record RolloverResult(
    RolloverDecision Decision,
    Result Result);