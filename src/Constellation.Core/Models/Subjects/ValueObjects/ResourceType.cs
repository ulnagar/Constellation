namespace Constellation.Core.Models.Subjects.ValueObjects;

using Constellation.Core.Primitives;
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

    private ResourceType(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}