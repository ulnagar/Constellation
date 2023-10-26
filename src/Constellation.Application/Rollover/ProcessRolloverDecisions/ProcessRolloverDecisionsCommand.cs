namespace Constellation.Application.Rollover.ProcessRolloverDecisions;

using Abstractions.Messaging;
using System.Collections.Generic;

public sealed record ProcessRolloverDecisionsCommand()
    : ICommand<List<RolloverResult>>;