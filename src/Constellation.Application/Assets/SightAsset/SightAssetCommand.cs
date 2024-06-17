namespace Constellation.Application.Assets.SightAsset;

using Abstractions.Messaging;
using Core.Models.Assets.ValueObjects;
using System;

public sealed record SightAssetCommand(
    AssetNumber AssetNumber,
    string StaffId,
    DateTime SightedAt,
    string Note)
    : ICommand;
