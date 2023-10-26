namespace Constellation.Core.Models.Rollover.Repositories;

using Constellation.Core.Models.Rollover;
using Constellation.Core.Shared;
using System.Collections.Generic;

public interface IRolloverRepository
{
    Result RegisterDecision(RolloverDecision rolloverDecision);
    void Reset();
    void Remove(RolloverDecision rolloverDecision);
    List<RolloverDecision> GetRegisteredDecisions();
}