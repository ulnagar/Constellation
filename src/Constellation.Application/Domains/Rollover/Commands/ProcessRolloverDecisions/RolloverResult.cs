namespace Constellation.Application.Domains.Rollover.Commands.ProcessRolloverDecisions;

using Core.Models.Rollover;
using Core.Shared;

public sealed record RolloverResult(
    RolloverDecision Decision,
    Result Result);