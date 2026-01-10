namespace Constellation.Application.Models.Identity.Enums;

using Core.Common;

public sealed class AppRoleType : StringEnumeration<AppRoleType>
{
    public static readonly AppRoleType None = new("");

    public static readonly AppRoleType Staff = new("Staff");
    public static readonly AppRoleType Student = new("Student");
    public static readonly AppRoleType Parent = new("Parent");
    public static readonly AppRoleType Contact = new("Contact");

    private AppRoleType(string value)
        : base(value, value) { }

    public static IEnumerable<AppRoleType> GetOptions => GetEnumerable;
}