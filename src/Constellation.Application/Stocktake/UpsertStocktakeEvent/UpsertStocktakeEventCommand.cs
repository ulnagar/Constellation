namespace Constellation.Application.Stocktake.UpsertStocktakeEvent;

using Abstractions.Messaging;
using System;

public sealed record UpsertStocktakeEventCommand(
    Guid? Id,
    string Name,
    DateTime StartDate,
    DateTime EndDate,
    bool AcceptLateResponses)
    : ICommand;