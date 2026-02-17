namespace Constellation.Application.Domains.AppSettings.Models;

using Core.Enums;
using Core.Models.AppSettings.Enums;
using Core.Models.StaffMembers;

public sealed record WorkflowConfiguration
{
    public WorkflowConfiguration(
        WorkflowArea position,
        Dictionary<StaffMember, List<Grade>> contacts)
    {
        Position = position;
        Contacts = contacts;
    }

    public WorkflowArea Position { get; init; }
    public IReadOnlyDictionary<StaffMember, List<Grade>> Contacts { get; }
}