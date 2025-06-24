namespace Constellation.Application.Domains.AssetManagement.Stocktake.Models;

using Core.Models.Stocktake.Identifiers;
using System;

public sealed record StocktakeEventResponse(
    StocktakeEventId Id,
    string Name,
    DateTime StartDate,
    DateTime EndDate,
    bool AcceptLateResponses);