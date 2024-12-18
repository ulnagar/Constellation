namespace Constellation.Infrastructure.Persistence.ConstellationContext.Converters;

using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Reflection;

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
                ContractResolver = new PrivateMemberContractResolver(),
                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
                TypeNameHandling = TypeNameHandling.All,
                NullValueHandling = NullValueHandling.Ignore
            });
}

internal sealed class PrivateMemberContractResolver
    : DefaultContractResolver
{
    protected override JsonProperty CreateProperty(
        MemberInfo member,
        MemberSerialization memberSerialization)
    {
        JsonProperty prop = base.CreateProperty(member, memberSerialization);

        if (!prop.Writable)
        {
            PropertyInfo property = member as PropertyInfo;
            if (property != null)
            {
                bool hasPrivateSetter = property.GetSetMethod(true) != null;
                prop.Writable = hasPrivateSetter;
            }
        }

        return prop;
    }
}