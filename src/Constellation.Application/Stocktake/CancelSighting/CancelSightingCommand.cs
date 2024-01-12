namespace Constellation.Application.Stocktake.CancelSighting;

using Abstractions.Messaging;
using System;

public sealed record CancelSightingCommand(
    Guid SightingId,
    string Comment,
    string CancelledBy,
    DateTime CancelledAt)
    : ICommand;