namespace Constellation.Application.Domains.AssetManagement.Assets.Commands.TransferAsset;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Assets.ValueObjects;
using System;

public sealed record TransferAssetToCoordinatingOfficeCommand(
    AssetNumber AssetNumber,
    string Room,
    bool CurrentLocation,
    DateOnly ArrivalDate)
    : ICommand;