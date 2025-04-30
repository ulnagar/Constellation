namespace Constellation.Application.Assets.TransferAsset;

using Abstractions.Messaging;
using Core.Models.Assets.ValueObjects;
using System;

public sealed record TransferAssetToCorporateOfficeCommand(
    AssetNumber AssetNumber,
    string Site,
    bool CurrentLocation,
    DateOnly ArrivalDate)
    : ICommand;