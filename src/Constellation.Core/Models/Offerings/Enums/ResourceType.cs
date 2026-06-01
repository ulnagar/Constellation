namespace Constellation.Core.Models.Offerings.Enums;

using Core.Common;

public sealed class ResourceType : StringEnumeration<ResourceType>
{
    public static readonly ResourceType Empty = new(string.Empty);

    public static readonly ResourceType MicrosoftTeam = new("Microsoft Team");
    public static readonly ResourceType CanvasCourse = new("Canvas Course");

    public ResourceType(string value)
        : base(value, value)
    {
    }

    public static IEnumerable<ResourceType> GetOptions => GetEnumerable;
}
