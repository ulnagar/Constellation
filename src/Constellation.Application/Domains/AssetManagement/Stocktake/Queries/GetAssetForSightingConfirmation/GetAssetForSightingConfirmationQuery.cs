namespace Constellation.Application.Domains.AssetManagement.Stocktake.Queries.GetAssetForSightingConfirmation;

using Abstractions.Messaging;
using Core.Models.Assets.ValueObjects;

public sealed record GetAssetForSightingConfirmationQuery(
    AssetNumber AssetNumber,
    string SerialNumber)
    : IQuery<AssetSightingResponse>;
