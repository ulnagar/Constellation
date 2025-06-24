namespace Constellation.Application.Domains.AssetManagement.Stocktake.Commands.RegisterSighting;

using Abstractions.Messaging;
using Core.Models.Assets.ValueObjects;
using Core.Models.Stocktake.Enums;
using Core.Models.Stocktake.Identifiers;
using System;

public sealed record RegisterSightingCommand(
    StocktakeEventId StocktakeEventId,
    string SerialNumber,
    AssetNumber AssetNumber,
    string Description,
    LocationCategory LocationCategory,
    string LocationName,
    string LocationCode,
    UserType UserType,
    string UserName,
    string UserCode,
    string Comment,
    string SightedBy,
    DateTime SightedAt)
    : ICommand;