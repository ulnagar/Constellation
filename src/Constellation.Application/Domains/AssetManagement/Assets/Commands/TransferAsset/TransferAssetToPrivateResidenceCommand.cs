namespace Constellation.Application.Assets.TransferAsset;

using Abstractions.Messaging;
using Core.Models.Assets.ValueObjects;
using System;

public sealed record TransferAssetToPrivateResidenceCommand(
    AssetNumber AssetNumber,
    bool CurrentLocation,
    DateOnly ArrivalDate)
    : ICommand;