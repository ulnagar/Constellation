namespace Constellation.Core.Models.Operations.Enums;

using Constellation.Core.Common;
using System.Collections.Generic;

public sealed class TeamsPermission : StringEnumeration<TeamsPermission>
{
    public static readonly TeamsPermission None = new("");
    public static readonly TeamsPermission Owner = new("Owner");
    public static readonly TeamsPermission Member = new("Member");

    private TeamsPermission(string value)
        : base(value, value) { }

    public static IEnumerable<TeamsPermission> GetOptions => GetEnumerable;
}