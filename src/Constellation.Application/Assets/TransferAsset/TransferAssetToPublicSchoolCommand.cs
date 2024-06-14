namespace Constellation.Application.Assets.TransferAsset;

using Abstractions.Messaging;
using Core.Models.Assets.ValueObjects;
using System;

public sealed record TransferAssetToPublicSchoolCommand(
    AssetNumber AssetNumber,
    string SchoolCode,
    bool CurrentLocation,
    DateOnly ArrivalDate)
    : ICommand;