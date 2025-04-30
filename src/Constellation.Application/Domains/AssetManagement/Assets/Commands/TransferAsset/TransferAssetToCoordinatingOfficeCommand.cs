namespace Constellation.Application.Assets.TransferAsset;

using Abstractions.Messaging;
using Core.Models.Assets.ValueObjects;
using System;

public sealed record TransferAssetToCoordinatingOfficeCommand(
    AssetNumber AssetNumber,
    string Room,
    bool CurrentLocation,
    DateOnly ArrivalDate)
    : ICommand;