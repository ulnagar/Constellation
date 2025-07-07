namespace Constellation.Infrastructure.ExternalServices.LissServer.Models;

using System.Text.Json;
using System.Text.Json.Serialization;

public sealed class LissPublishDailyData
{
    public DateTime Date { get; set; }
    public string Period { get; set; }
    public string StartTime { get; set; }
    public string EndTime { get; set; }
    public string ClassCode { get; set; }
    public string EdvalClassCode { get; set; }
    public Guid Guid { get; set; }
    public string Rooms { get; set; }
    public string TeacherCodes { get; set; }
    public string TeacherIds { get; set; }
    public string ClassName { get; set; }
    public string Replacing { get; set; }
    public string Type { get; set; }
    [JsonPropertyName("TtStructure")]
    public string Timetable { get; set; }
}

public class CustomLissDateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string value = reader.GetString();

        string year = value[..4];
        string month = value[4..6];
        string day = value[6..8];

        DateTime dateTime = DateTime.Parse($"{year}-{month}-{day}");
        return dateTime;
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}