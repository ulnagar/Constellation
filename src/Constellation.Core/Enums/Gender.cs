namespace Constellation.Core.Enums;

using Common;
using System.Collections.Generic;

public sealed class Gender : StringEnumeration<Gender>
{
    public static readonly Gender Unknown = new("Unknown");

    public static readonly Gender Male = new("Male");
    public static readonly Gender Female = new("Female");
    public static readonly Gender NonBinary = new("Non-Binary");

    private Gender(string value)
        : base(value, value) { }

    public static IEnumerable<Gender> GetOptions => GetEnumerable;
}