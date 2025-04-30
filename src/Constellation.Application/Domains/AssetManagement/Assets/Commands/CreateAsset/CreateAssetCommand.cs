namespace Constellation.Application.Assets.CreateAsset;

using Abstractions.Messaging;
using Core.Models.Assets.Enums;
using Core.Models.Assets.ValueObjects;

public sealed record CreateAssetCommand(
    AssetNumber AssetNumber,
    string SerialNumber,
    string Manufacturer,
    string Description,
    AssetCategory Category)
    : ICommand;