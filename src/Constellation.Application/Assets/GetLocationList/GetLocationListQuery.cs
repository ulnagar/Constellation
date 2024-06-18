namespace Constellation.Application.Assets.GetLocationList;

using Abstractions.Messaging;
using Core.Models.Assets.Enums;
using System.Collections.Generic;

public sealed record GetLocationListQuery(
    LocationCategory Category)
    : IQuery<List<LocationListItem>>;