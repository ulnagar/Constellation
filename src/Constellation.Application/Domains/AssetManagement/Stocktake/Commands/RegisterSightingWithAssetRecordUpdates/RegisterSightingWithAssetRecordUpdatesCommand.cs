namespace Constellation.Application.Domains.AssetManagement.Stocktake.Commands.RegisterSightingWithAssetRecordUpdates;

using Abstractions.Messaging;
using Constellation.Core.Models.Stocktake.Identifiers;
using Core.Models.Assets.ValueObjects;
using Core.Models.Stocktake.Enums;
using System;

public sealed record RegisterSightingWithAssetRecordUpdatesCommand(
    StocktakeEventId StocktakeEventId,
    AssetNumber AssetNumber,
    LocationCategory LocationCategory,
    string LocationName,
    string LocationCode,
    UserType UserType,
    string UserName,
    string UserCode,
    string Comment)
    : ICommand;
