namespace Constellation.Application.Domains.AssetManagement.Stocktake.Commands.UpsertStocktakeEvent;

using Abstractions.Messaging;
using Core.Models.Stocktake.Identifiers;
using System;

public sealed record UpsertStocktakeEventCommand(
    StocktakeEventId Id,
    string Name,
    DateTime StartDate,
    DateTime EndDate,
    bool AcceptLateResponses)
    : ICommand;