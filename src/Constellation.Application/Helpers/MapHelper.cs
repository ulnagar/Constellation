namespace Constellation.Application.Helpers;

using Constellation.Application.DTOs;
using Constellation.Core.Models;
using System.Collections.Generic;
using System.Linq;

public static class MapHelpers
{
    public static MapLayer MapLayerBuilder(IQueryable<School> schools, string name, string colour)
    {
        List<MapItem> markers = schools
            .Select(school => MapItem.ConvertFromSchool(school))
            .ToList();
            
        MapLayer layer = new()
        {
            Colour = colour,
            Name = name
        };

        markers.ForEach(marker => layer.AddMarker(marker));

        return layer;
    }
}