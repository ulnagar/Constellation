namespace Constellation.Application.Domains.AssetManagement.Stocktake.Commands.RegisterSighting;

using Abstractions.Messaging;
using System;

public sealed record RegisterSightingCommand(
    Guid StocktakeEventId,
    string SerialNumber,
    string AssetNumber,
    string Description,
    string LocationCategory,
    string LocationName,
    string LocationCode,
    string UserType,
    string UserName,
    string UserCode,
    string Comment,
    string SightedBy,
    DateTime SightedAt)
    : ICommand;