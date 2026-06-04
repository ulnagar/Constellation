namespace Constellation.Core.Models.StudentOnboarding.Policy;

using Enums;
using System.Collections.Generic;
using static Enums.ApplicationPhase;
using static Enums.ApplicationStatus;

public static class ApplicationTransitions
{
    private static ApplicationState S(ApplicationPhase phase, ApplicationStatus status) => ApplicationState.Unsafe(phase, status);

    private static readonly Dictionary<ApplicationState, HashSet<ApplicationState>> Table;

    static ApplicationTransitions()
    {
        var pairs = new(ApplicationState From, ApplicationState To)[]
        {
            // DataEntry
            (S(DataEntry, Pending), S(DataEntry, Accepted)),
            (S(DataEntry, Pending), S(DataEntry, Declined)),
            (S(DataEntry, Pending), S(DataEntry, Lapsed)),
            (S(DataEntry, Accepted), S(Placement, Pending)),

            // Placement
            (S(Placement, Pending),  S(Placement, Accepted)),
            (S(Placement, Pending),  S(Placement, Declined)),
            (S(Placement, Pending),  S(Placement, Lapsed)),
            (S(Placement, Accepted), S(Approval,  Pending)),

            // Approval
            (S(Approval, Pending),   S(Approval,  Accepted)),
            (S(Approval, Pending),   S(Approval,  Declined)),
            (S(Approval, Pending),   S(Approval,  Lapsed)),
            (S(Approval, Accepted),  S(Processing, Pending)),

            // Processing — terminal phase, no advancement
            (S(Processing, Pending), S(Processing, Accepted)),
            (S(Processing, Pending), S(Processing, Declined)),
            (S(Processing, Pending), S(Processing, Lapsed)),
        };

        Table = pairs
            .GroupBy(p => p.From)
            .ToDictionary(
                g => g.Key, 
                g => g.Select(p => p.To).ToHashSet());
    }

    public static bool IsValid(ApplicationState from, ApplicationState to)
        => Table.TryGetValue(from, out var targets) && targets.Contains(to);

    public static IReadOnlyCollection<ApplicationState> ValidTransitionsFrom(ApplicationState state)
        => Table.TryGetValue(state, out var targets)
            ? targets
            : Array.Empty<ApplicationState>();
}
