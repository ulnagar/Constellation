namespace Constellation.Application.Helpers.JsonConverters;

using Constellation.Core.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Reflection;

public sealed class IntEnumerationJsonConverter : JsonConverter
{
    public override bool CanConvert(Type objectType) => IsIntEnumeration(objectType);

    private static bool IsIntEnumeration(Type type)
    {
        for (var t = type; t is not null && t != typeof(object); t = t.BaseType)
        {
            if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IntEnumeration<>))
                return true;
        }
        return false;
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value is null)
        {
            writer.WriteNull();
            return;
        }

        var intValue = (int)value.GetType().GetProperty("Value")!.GetValue(value)!;
        writer.WriteValue(intValue);
    }

    public override object? ReadJson(
        JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
            return null;

        int value = reader.TokenType switch
        {
            JsonToken.Integer => Convert.ToInt32(reader.Value),
            JsonToken.StartObject => JObject.Load(reader)["Value"]!.Value<int>(),
            _ => throw new JsonSerializationException(
                $"Unexpected token {reader.TokenType} reading {objectType.Name}")
        };

        return objectType
            .GetMethod("FromValue", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)!
            .Invoke(null, new object[] { value });
    }
}