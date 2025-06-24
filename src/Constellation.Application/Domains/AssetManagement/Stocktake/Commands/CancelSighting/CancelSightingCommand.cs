namespace Constellation.Application.Domains.AssetManagement.Stocktake.Commands.CancelSighting;

using Abstractions.Messaging;
using Core.Models.Stocktake.Identifiers;
using System;

public sealed record CancelSightingCommand(
    StocktakeEventId EventId,
    StocktakeSightingId SightingId,
    string Comment,
    string CancelledBy,
    DateTime CancelledAt)
    : ICommand;