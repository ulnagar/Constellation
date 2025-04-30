namespace Constellation.Application.Assets.DeallocateAsset;

using Constellation.Application.Abstractions.Messaging;
using Core.Models.Assets.ValueObjects;

public sealed record DeallocateAssetCommand(
    AssetNumber AssetNumber)
    : ICommand;