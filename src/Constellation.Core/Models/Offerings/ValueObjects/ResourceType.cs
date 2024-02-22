namespace Constellation.Core.Models.Offerings.ValueObjects;

using Primitives;
using Newtonsoft.Json;
using System.Collections.Generic;

public sealed class ResourceType : ValueObject
{
    public static readonly ResourceType AdobeConnectRoom = new("Adobe Connect Room");
    public static readonly ResourceType MicrosoftTeam = new("Microsoft Team");
    public static readonly ResourceType CanvasCourse = new("Canvas Course");

    public static ResourceType FromValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return new(value);
    }

    [JsonConstructor]
    private ResourceType(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(ResourceType resourceType) =>
        resourceType is null ? string.Empty : resourceType.ToString();
}
