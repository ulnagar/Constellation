namespace Constellation.Application.Domains.AssetManagement.Stocktake.Commands.RegisterManualSighting;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Stocktake.Enums;
using Constellation.Core.Models.Stocktake.Identifiers;
using System;

public sealed record RegisterManualSightingCommand(
    StocktakeEventId StocktakeEventId,
    string SerialNumber,
    string Description,
    LocationCategory LocationCategory,
    string LocationName,
    string LocationCode,
    UserType UserType,
    string UserName,
    string UserCode,
    string Comment)
    : ICommand;