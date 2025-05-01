namespace Constellation.Application.Domains.Rollover.Commands.ProcessRolloverDecisions;

using Abstractions.Messaging;
using System.Collections.Generic;

public sealed record ProcessRolloverDecisionsCommand()
    : ICommand<List<RolloverResult>>;