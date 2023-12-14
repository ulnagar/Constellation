namespace Constellation.Application.DTOs;

using Constellation.Core.Models;
using System.Collections.Generic;
using System.Linq;

public class MapLayer
{
    public string Name { get; set; }
    public string Colour { get; set; }
    public List<MapItem> Markers { get; set; } = new();
    
    public void AddMarker(MapItem marker)
    {
        if (marker.Colour != Colour)
            marker.Colour = Colour;

        Markers.Add(marker);
    }
}

public class MapItem
{
    public string Description { get; set; }

    public double X { get; set; }

    public double Y { get; set; }

    public bool ShowPopup { get; set; } = false;
    public string Colour { get; set; }

    public static MapItem ConvertFromSchool(School school) =>
        new()
        {
            Description = $"<strong>{school.Name}</strong><br /><hr />Students: {school.Students.Count(student => !student.IsDeleted)}<br />Staff: {school.Staff.Count(staff => !staff.IsDeleted)}",
            X = school.Latitude,
            Y = school.Longitude
        };
}