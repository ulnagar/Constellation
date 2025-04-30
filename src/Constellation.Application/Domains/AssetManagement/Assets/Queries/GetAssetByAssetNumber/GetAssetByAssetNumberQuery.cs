namespace Constellation.Application.Domains.AssetManagement.Assets.Queries.GetAssetByAssetNumber;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Assets.ValueObjects;

public sealed record GetAssetByAssetNumberQuery(
    AssetNumber AssetNumber)
    : IQuery<AssetResponse>;
