namespace Constellation.Application.Assets.GetAllActiveAssets;

using Abstractions.Messaging;
using Constellation.Application.Domains.AssetManagement.Assets.Models;
using Models;
using System.Collections.Generic;

public sealed record GetAllActiveAssetsQuery()
    : IQuery<List<AssetListItem>>;