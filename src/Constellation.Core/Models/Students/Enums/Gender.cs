namespace Constellation.Core.Models.Students.Enums;

using Constellation.Core.Common;
using System.Collections.Generic;

public sealed class Gender : StringEnumeration<Gender>
{
    public static readonly Gender Male = new("Male");
    public static readonly Gender Female = new("Female");
    public static readonly Gender NonBinary = new("Non-Binary");

    private Gender(string value)
        : base(value, value) { }

    public static IEnumerable<Gender> GetOptions => GetEnumerable;
}

public sealed class IndigenousStatus : IntEnumeration<IndigenousStatus>
{
    public static readonly IndigenousStatus AboriginalButNotTorresStraitIslander = new(1, "Aboriginal but not Torres Strait Islander origin");
    public static readonly IndigenousStatus TorresStraitIslanderButNotAboriginal = new(2, "Torres Strait Islander but not Aboriginal origin");
    public static readonly IndigenousStatus BothAboriginalAndTorresStraitIslander = new(3, "Both Aboriginal and Torres Strait Islander origin");
    public static readonly IndigenousStatus NeitherAboriginalNorTorresStraitIslander = new(4, "Neither Aboriginal nor Torres Strait Islander origin");
    public static readonly IndigenousStatus Unknown = new(9, "Unknown");

    private IndigenousStatus(int value, string name)
        : base(value, name) { }

    public static IEnumerable<IndigenousStatus> GetOptions => GetEnumerable;
}