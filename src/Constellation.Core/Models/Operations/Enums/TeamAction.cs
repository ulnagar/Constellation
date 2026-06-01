namespace Constellation.Core.Models.Operations.Enums;

using Core.Common;

public sealed class TeamAction : StringEnumeration<TeamAction>
{
    public static readonly TeamAction AddMember = new("AddMember", "Add as Member");
    public static readonly TeamAction AddOwner = new("AddOwner", "Add as Owner");
    public static readonly TeamAction Remove = new("Remove", "Remove");

    private TeamAction(string value, string name)
        : base(value, name) { }
}