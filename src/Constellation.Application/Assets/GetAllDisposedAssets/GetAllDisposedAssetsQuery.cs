namespace Constellation.Application.Assets.GetAllDisposedAssets;

using Abstractions.Messaging;
using Models;
using System.Collections.Generic;

public sealed record GetAllDisposedAssetsQuery()
    : IQuery<List<AssetListItem>>;