namespace Constellation.Application.Assets.GetAssetByAssetNumber;

using Abstractions.Messaging;
using Core.Models.Assets.ValueObjects;

public sealed record GetAssetByAssetNumberQuery(
    AssetNumber AssetNumber)
    : IQuery<AssetResponse>;
