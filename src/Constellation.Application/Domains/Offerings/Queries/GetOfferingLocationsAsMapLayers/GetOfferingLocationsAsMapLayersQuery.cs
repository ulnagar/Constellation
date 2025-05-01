namespace Constellation.Application.Domains.Offerings.Queries.GetOfferingLocationsAsMapLayers;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.DTOs;
using Constellation.Core.Models.Offerings.Identifiers;
using System.Collections.Generic;

public sealed record GetOfferingLocationsAsMapLayersQuery(
    OfferingId OfferingId)
    : IQuery<List<MapLayer>>;
