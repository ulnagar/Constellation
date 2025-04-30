namespace Constellation.Application.Domains.AssetManagement.Assets.Queries.GetAllAssets;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Domains.AssetManagement.Assets.Models;
using System.Collections.Generic;

public sealed record GetAllAssetsQuery()
    : IQuery<List<AssetListItem>>;