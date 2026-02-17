namespace Constellation.Application.Domains.AppSettings.Models;

using Core.Enums;
using Core.Models.StaffMembers;

public sealed record MandatoryTrainingConfiguration
{
    public MandatoryTrainingConfiguration(
        Dictionary<StaffMember, List<Grade>> contacts)
    {
        Contacts = contacts;
    }

    public IReadOnlyDictionary<StaffMember, List<Grade>> Contacts { get; }
}