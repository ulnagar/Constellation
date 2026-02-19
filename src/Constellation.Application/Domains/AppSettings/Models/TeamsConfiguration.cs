namespace Constellation.Application.Domains.AppSettings.Models;

using Core.Enums;
using Core.Models.StaffMembers;
using Core.ValueObjects;

public sealed record TeamsConfiguration
{
    public TeamsConfiguration(
        Dictionary<StaffMember,List<Grade>> mandatoryOwners,
        Dictionary<StaffMember,List<Grade>> studentTeamOwners,
        Dictionary<StaffMember, List<Grade>> studentChannelOwners)
    {
        MandatoryOwners = mandatoryOwners;
        StudentTeamOwners = studentTeamOwners;
        StudentChannelOwners = studentChannelOwners;
    }

    public IReadOnlyDictionary<StaffMember, List<Grade>> MandatoryOwners { get; }
    public IReadOnlyDictionary<StaffMember, List<Grade>> StudentTeamOwners { get; }
    public IReadOnlyDictionary<StaffMember, List<Grade>> StudentChannelOwners { get; }

    public static IReadOnlyList<EmailAddress> FallbackMandatoryOwners => new List<EmailAddress>
    {
        EmailAddress.FromValue("nhi.auroracollege@det.nsw.edu.au"),
        EmailAddress.FromValue("michael.necovski2@det.nsw.edu.au"),
        EmailAddress.FromValue("benjamin.hillsley@det.nsw.edu.au")
    };
}