namespace Constellation.Application.Helpers.JsonConverters;

using Constellation.Core.Shared;
using Core.Enums;
using Newtonsoft.Json;
using System;
using System.Text.Json;
using JsonException = System.Text.Json.JsonException;
using NewtonsoftConverter = Newtonsoft.Json.JsonConverter<Core.Enums.Grade>;
using STJConverter = System.Text.Json.Serialization.JsonConverter<Core.Enums.Grade>;


/// <summary>
/// System.Text.Json converter for <see cref="Grade"/>. Serializes to/from the
/// grade's string value (e.g. "Y07"); reads legacy raw-int values for
/// backward compatibility with data written before the Grade string conversion.
/// </summary>
public sealed class GradeSystemTextJsonConverter : STJConverter
{
    public override Grade Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
        {
            int number = reader.GetInt32();
            Result<Grade> result = Grade.FromNumber(number);
            return result.IsSuccess ? result.Value : Grade.Empty;
        }

        if (reader.TokenType == JsonTokenType.String)
        {
            string value = reader.GetString();

            if (string.IsNullOrEmpty(value))
                return Grade.Empty;

            Grade grade = Grade.FromValue(value);
            return grade ?? Grade.Empty;
        }

        if (reader.TokenType == JsonTokenType.Null)
            return Grade.Empty;

        throw new JsonException($"Unexpected token {reader.TokenType} when parsing Grade.");
    }

    public override void Write(Utf8JsonWriter writer, Grade value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value == Grade.Empty ? string.Empty : value.Value);
    }
}

/// <summary>
/// Newtonsoft.Json converter for <see cref="Grade"/>. Serializes to/from the
/// grade's string value (e.g. "Y07"); reads legacy raw-int values for
/// backward compatibility with data written before the Grade string conversion.
/// </summary>
public sealed class GradeNewtonsoftJsonConverter : NewtonsoftConverter
{
    public override void WriteJson(
        Newtonsoft.Json.JsonWriter writer,
        Grade value,
        Newtonsoft.Json.JsonSerializer serializer)
    {
        writer.WriteValue(value == Grade.Empty ? string.Empty : value.Value);
    }

    public override Grade ReadJson(
        Newtonsoft.Json.JsonReader reader,
        Type objectType,
        Grade existingValue,
        bool hasExistingValue,
        Newtonsoft.Json.JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Integer)
        {
            int number = Convert.ToInt32(reader.Value);
            Result<Grade> result = Grade.FromNumber(number);
            return result.IsSuccess ? result.Value : Grade.Empty;
        }

        if (reader.TokenType == JsonToken.String)
        {
            string value = (string)reader.Value;

            if (string.IsNullOrEmpty(value))
                return Grade.Empty;

            Grade grade = Grade.FromValue(value);
            return grade ?? Grade.Empty;
        }

        return Grade.Empty;
    }
}