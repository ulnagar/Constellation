namespace Constellation.Application.Schools.GetSchoolLocationsAsMapLayers;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.DTOs;
using System.Collections.Generic;


public sealed record GetSchoolLocationsAsMapLayersQuery(
    List<string> SchoolCodes)
    : IQuery<List<MapLayer>>;