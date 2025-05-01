namespace Constellation.Application.Domains.Schools.Queries.GetSchoolLocationsAsMapLayers;

using Abstractions.Messaging;
using DTOs;
using System.Collections.Generic;

public sealed record GetSchoolLocationsAsMapLayersQuery(
    List<string> SchoolCodes)
    : IQuery<List<MapLayer>>;