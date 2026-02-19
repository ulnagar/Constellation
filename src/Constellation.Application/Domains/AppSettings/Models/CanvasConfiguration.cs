namespace Constellation.Application.Domains.AppSettings.Models;

using Core.Enums;
using Core.Models.AppSettings;
using Core.Models.StaffMembers;

public sealed record CanvasConfiguration
{
    public CanvasConfiguration(
        CanvasSettings settings,
        Dictionary<StaffMember, List<Grade>> admins)
    {
        UseGroups = settings.UseGroups;
        UseSections = settings.UseSections;
        Admins = admins;
    }

    public CanvasConfiguration(
        bool useGroups,
        bool useSections,
        Dictionary<StaffMember, List<Grade>> admins)
    {
        UseGroups = useGroups;
        UseSections = useSections;
        Admins = admins;
    }

    public bool UseGroups { get; init; }
    public bool UseSections { get; init; }
    public IReadOnlyDictionary<StaffMember, List<Grade>> Admins { get; }
}