namespace Constellation.Application.Domains.AssetManagement.Assets.Commands.UpdateAssetStatus;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Assets.Enums;
using Constellation.Core.Models.Assets.ValueObjects;

public sealed record UpdateAssetStatusCommand(
    AssetNumber AssetNumber,
    AssetStatus AssetStatus)
    : ICommand;