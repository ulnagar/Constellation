namespace Constellation.Core.Models.Rollover.Repositories;

using Rollover;
using Shared;
using System.Collections.Generic;

public interface IRolloverRepository
{
    Result RegisterDecision(RolloverDecision rolloverDecision);
    void Reset();
    void Remove(RolloverDecision rolloverDecision);
    List<RolloverDecision> GetRegisteredDecisions();
}