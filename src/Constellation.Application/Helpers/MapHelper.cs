using Constellation.Application.DTOs;
using Constellation.Core.Models;
using System.Linq;

namespace Constellation.Application.Helpers
{
    public static class MapHelpers
    {
        public static MapLayer MapLayerBuilder(IQueryable<School> schools, string name, string colour)
        {
            var markers = schools.Select(school => MapItem.ConvertFromSchool(school)).ToList();
            
            var layer = new MapLayer
            {
                Colour = colour,
                Name = name
            };

            markers.ForEach(marker => layer.AddMarker(marker));

            return layer;
        }
    }
}
