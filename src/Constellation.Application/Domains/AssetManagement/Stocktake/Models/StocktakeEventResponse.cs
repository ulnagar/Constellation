namespace Constellation.Application.Domains.AssetManagement.Stocktake.Models;

using System;

public sealed record StocktakeEventResponse(
    Guid Id,
    string Name,
    DateTime StartDate,
    DateTime EndDate,
    bool AcceptLateResponses);