namespace Constellation.Core.Models.Students.Enums;

using Common;
using System.Collections.Generic;

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