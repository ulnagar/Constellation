namespace Constellation.Application.Assets.GetAllAssets;

using Abstractions.Messaging;
using Models;
using System.Collections.Generic;

public sealed record GetAllAssetsQuery()
    : IQuery<List<AssetListItem>>;