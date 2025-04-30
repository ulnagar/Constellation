namespace Constellation.Application.Domains.AssetManagement.Stocktake.Commands.CancelSighting;

using Abstractions.Messaging;
using System;

public sealed record CancelSightingCommand(
    Guid SightingId,
    string Comment,
    string CancelledBy,
    DateTime CancelledAt)
    : ICommand;