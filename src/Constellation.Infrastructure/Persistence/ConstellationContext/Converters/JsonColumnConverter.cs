namespace Constellation.Infrastructure.Persistence.ConstellationContext.Converters;

using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;

internal sealed class JsonColumnConverter<T> : ValueConverter<T, string?>
{
    public JsonColumnConverter()
        : base(
            original => TypeToJson(original),
            value => JsonToType(value),
            new ConverterMappingHints())
    { }

    private static string? TypeToJson(T original) =>
        JsonConvert.SerializeObject(
            original,
            new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                NullValueHandling = NullValueHandling.Ignore
            });

    private static T JsonToType(string? value) =>
        JsonConvert.DeserializeObject<T>(
            value,
            new JsonSerializerSettings
            {
                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
                TypeNameHandling = TypeNameHandling.All,
                NullValueHandling = NullValueHandling.Ignore
            });
}