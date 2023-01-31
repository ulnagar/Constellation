namespace Constellation.Core.Enums;

using Constellation.Core.Common;

public class TeamsMembershipLevel : StringEnumeration<TeamsMembershipLevel>
{
    public static readonly TeamsMembershipLevel Member = new("Member", "Member");
    public static readonly TeamsMembershipLevel Owner = new("Owner", "Owner");
    public static readonly TeamsMembershipLevel None = new("None", "None");

    private TeamsMembershipLevel(string value, string name)
        : base(value, name)
    { }
}
