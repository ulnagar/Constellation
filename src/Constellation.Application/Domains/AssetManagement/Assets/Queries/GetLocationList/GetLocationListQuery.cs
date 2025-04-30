namespace Constellation.Application.Domains.AssetManagement.Assets.Queries.GetLocationList;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Assets.Enums;
using System.Collections.Generic;

public sealed record GetLocationListQuery(
    LocationCategory Category)
    : IQuery<List<LocationListItem>>;