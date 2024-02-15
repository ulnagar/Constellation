namespace Constellation.Application.Extensions;

using System.Text.Json;

public static class JsonSerializerExtensions
{
    public static T? DeserializeAnonymousType<T>(string json, T anonymousTypeObject, JsonSerializerOptions? options = default)
        => JsonSerializer.Deserialize<T>(json, options);
}
