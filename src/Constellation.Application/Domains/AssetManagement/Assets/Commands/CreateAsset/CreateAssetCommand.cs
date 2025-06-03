namespace Constellation.Application.Domains.AssetManagement.Assets.Commands.CreateAsset;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Assets.Enums;
using Constellation.Core.Models.Assets.ValueObjects;

public sealed record CreateAssetCommand(
    AssetNumber AssetNumber,
    string SerialNumber,
    string Manufacturer,
    string Description,
    AssetCategory Category)
    : ICommand;