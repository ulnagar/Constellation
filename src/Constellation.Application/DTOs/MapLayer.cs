namespace Constellation.Application.DTOs;

using System.Collections.Generic;

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
    public string Description => $"<strong>{SchoolName}</strong><br /><hr />Students: {StudentCount}<br />Staff: {StaffCount}";

    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public bool ShowPopup { get; set; }
    public string Colour { get; set; } = string.Empty;

    public string SchoolName { get; set; } = string.Empty;
    public int StudentCount { get; set; }
    public int StaffCount { get; set; }
}