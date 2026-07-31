namespace Constellation.Infrastructure.Persistence.EnrolmentContext.Converters;

using ConstellationContext.Converters;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;
using System.Collections.Generic;

internal sealed class JsonListConverter<TItem> : ValueConverter<IReadOnlyList<TItem>, string>
{
    public JsonListConverter()
        : base(
            list => Serialize(list),
            json => Deserialize(json))
    { }

    private static string Serialize(IReadOnlyList<TItem> list) =>
        JsonConvert.SerializeObject(list ?? new List<TItem>(), Settings);

    private static List<TItem> Deserialize(string json) =>
        string.IsNullOrWhiteSpace(json)
            ? new List<TItem>()
            : JsonConvert.DeserializeObject<List<TItem>>(json, Settings) ?? new List<TItem>();

    private static readonly JsonSerializerSettings Settings = new()
    {
        ContractResolver = new PrivateMemberContractResolver()
    };
}

internal sealed class JsonListComparer<TItem> : ValueComparer<IReadOnlyList<TItem>>
{
    public JsonListComparer()
        : base(
            (a, b) => (a ?? new List<TItem>()).SequenceEqual(b ?? new List<TItem>()),
            list => (list ?? new List<TItem>()).Aggregate(0, (hash, item) => HashCode.Combine(hash, item)),
            list => (list ?? new List<TItem>()).ToList())
    { }
}
