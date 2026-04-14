namespace Constellation.Core.Models.Auth.Enums;

using Constellation.Core.Common;

public sealed class LinkType : StringEnumeration<LinkType>
{
    public static readonly LinkType None = new("");

    public static readonly LinkType Staff = new("Staff");
    public static readonly LinkType Student = new("Student");
    public static readonly LinkType Parent = new("Parent");
    public static readonly LinkType Family = new("Family");
    public static readonly LinkType Contact = new("Contact");

    private LinkType(string value)
        : base(value, value) { }

    public static IEnumerable<LinkType> GetOptions => GetEnumerable;
}