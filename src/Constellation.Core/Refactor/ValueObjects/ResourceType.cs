namespace Constellation.Core.Refactor.ValueObjects;

using Constellation.Core.Refactor.Common;
using Constellation.Core.Refactor.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;

public class ResourceType : ValueObject
{
    static ResourceType() { }

    private ResourceType() { }

    private ResourceType(string code)
    {
        Code = code;
    }

    public static ResourceType From(string code)
    {
        var resourceType = new ResourceType { Code = code };

        if (!SupportedResourceTypes.Contains(resourceType))
        {
            throw new UnsupportedResourceTypeException(code);
        }

        return resourceType;
    }

    public static ResourceType MSTeams => new("Microsoft Teams");
    public static ResourceType AdobeConnect => new("Adobe Connect");
    public static ResourceType Canvas => new("Canvas");
    public static ResourceType Unknown => new("Unknown");
    public string Code { get; private set; } = "Unknown";

    public static implicit operator string(ResourceType classType)
    {
        return classType.ToString();
    }

    public static explicit operator ResourceType(string code)
    {
        return From(code);
    }

    public override string ToString()
    {
        return Code;
    }

    protected static IEnumerable<ResourceType> SupportedResourceTypes
    {
        get
        {
            yield return MSTeams;
            yield return AdobeConnect;
            yield return Canvas;
            yield return Unknown;
        }
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Code;
    }

}
