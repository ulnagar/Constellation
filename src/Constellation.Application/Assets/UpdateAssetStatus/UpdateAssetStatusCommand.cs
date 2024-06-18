namespace Constellation.Application.Assets.UpdateAssetStatus;

using Abstractions.Messaging;
using Core.Models.Assets.Enums;
using Core.Models.Assets.ValueObjects;

public sealed record UpdateAssetStatusCommand(
    AssetNumber AssetNumber,
    AssetStatus AssetStatus)
    : ICommand;