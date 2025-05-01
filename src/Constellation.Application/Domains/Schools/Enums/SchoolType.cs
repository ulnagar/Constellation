namespace Constellation.Application.Domains.Schools.Enums;

using Core.Common;

public sealed class SchoolType : StringEnumeration<SchoolType>
{
    public static readonly SchoolType Primary = new("Primary", "Primary School");
    public static readonly SchoolType Central = new("Central", "Central School");
    public static readonly SchoolType Secondary = new("Secondary", "Secondary School");

    public SchoolType(string value, string name) 
        : base(value, name) { }
}
