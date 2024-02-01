namespace Constellation.Infrastructure.ExternalServices.LissServer.Models;

using Constellation.Core.Primitives;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

public sealed class LissCallMethod : ValueObject
{
    public static readonly LissCallMethod Hello = new("liss.hello");
    public static readonly LissCallMethod GetStudents = new("liss.getStudents");
    public static readonly LissCallMethod PublishStudents = new("liss.publishStudents");
    public static readonly LissCallMethod GetTeachers = new("liss.getTeachers");
    public static readonly LissCallMethod PublishTeachers = new("liss.publishTeachers");
    public static readonly LissCallMethod GetRooms = new("liss.getRooms");
    public static readonly LissCallMethod PublishRooms = new("liss.publishRooms");
    public static readonly LissCallMethod GetClassMemberships = new("liss.getClassMemberships");
    public static readonly LissCallMethod PublishClassMemberships = new("liss.publishClassMemberships");
    public static readonly LissCallMethod GetClasses = new("liss.getClasses");
    public static readonly LissCallMethod PublishClasses = new("liss.publishClasses");
    public static readonly LissCallMethod GetTimetableStructures = new("liss.getTimetableStructures");
    public static readonly LissCallMethod GetTimetable = new("liss.getTimetable");
    public static readonly LissCallMethod PublishTimetable = new("liss.publishTimetable");
    public static readonly LissCallMethod PublishDailyData = new("liss.publishDailyData");
    public static readonly LissCallMethod GetBellTimes = new("liss.getBellTimes");
    public static readonly LissCallMethod PublishBellTimes = new("liss.publishBellTimes");
    public static readonly LissCallMethod GetCalendar = new("liss.getCalendar");
    public static readonly LissCallMethod PublishCalendar = new("liss.publishCalendar");
    public static readonly LissCallMethod ChangeClassMembership = new("liss.changeClassMembership");

    public static LissCallMethod FromValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return new(value);
    }

    private LissCallMethod(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}

public class LissCallMethodConverter : JsonConverter<LissCallMethod>
{
    public override LissCallMethod Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options) =>
        LissCallMethod.FromValue(reader.GetString());

    public override void Write(Utf8JsonWriter writer, LissCallMethod value, JsonSerializerOptions options) => writer.WriteStringValue(value.Value);
}