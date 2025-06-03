namespace Constellation.Application.Domains.AssetManagement.Assets.Queries.GetAllActiveAssets;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Domains.AssetManagement.Assets.Models;
using System.Collections.Generic;

public sealed record GetAllActiveAssetsQuery()
    : IQuery<List<AssetListItem>>;