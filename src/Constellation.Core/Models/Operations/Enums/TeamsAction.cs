namespace Constellation.Core.Models.Operations.Enums;

using Constellation.Core.Common;
using System.Collections.Generic;

public sealed class TeamsAction : StringEnumeration<TeamsAction>
{
    public static readonly TeamsAction None = new("");
    public static readonly TeamsAction Add = new("Add");
    public static readonly TeamsAction Remove = new("Remove");
    public static readonly TeamsAction AddTeam = new("Add Team");
    public static readonly TeamsAction AddChannel = new("Add Channel");

    private TeamsAction(string value)
        : base(value, value) { }

    public static IEnumerable<TeamsAction> GetOptions => GetEnumerable;
}