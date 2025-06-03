namespace Constellation.Application.Domains.AssetManagement.Assets.Commands.DeallocateAsset;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Assets.ValueObjects;

public sealed record DeallocateAssetCommand(
    AssetNumber AssetNumber)
    : ICommand;