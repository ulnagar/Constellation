namespace Constellation.Application.Domains.AssetManagement.Assets.Commands.TransferAsset;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Assets.ValueObjects;
using System;

public sealed record TransferAssetToPrivateResidenceCommand(
    AssetNumber AssetNumber,
    bool CurrentLocation,
    DateOnly ArrivalDate)
    : ICommand;