namespace Constellation.Infrastructure.ExternalServices.LissServer.Models;

using System.Text.Json.Serialization;

public sealed class LissPublishTimetable
{
    public int DayNumber { get; set; }
    public string Period { get; set; }
    public string ClassCode { get; set; }
    public string TeacherId { get; set; }
    public string RoomId { get; set; }
    public string RoomCode { get; set; }

    [JsonPropertyName("TtStructure")]
    public string Timetable { get; set; }
}