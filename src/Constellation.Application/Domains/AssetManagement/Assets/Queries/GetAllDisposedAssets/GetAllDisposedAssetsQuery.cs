namespace Constellation.Application.Domains.AssetManagement.Assets.Queries.GetAllDisposedAssets;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Domains.AssetManagement.Assets.Models;
using System.Collections.Generic;

public sealed record GetAllDisposedAssetsQuery()
    : IQuery<List<AssetListItem>>;