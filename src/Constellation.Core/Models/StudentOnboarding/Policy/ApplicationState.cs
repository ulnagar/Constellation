namespace Constellation.Core.Models.StudentOnboarding.Policy;

using Enums;
using Errors;
using Shared;
using static Enums.ApplicationPhase;
using static Enums.ApplicationStatus;

public sealed record ApplicationState
{
    private static readonly Dictionary<ApplicationPhase, IReadOnlySet<ApplicationStatus>> _validStatuses =
        new()
        {
            [DataEntry] = new HashSet<ApplicationStatus> { Pending, Accepted, Declined, Lapsed },
            [Placement] = new HashSet<ApplicationStatus> { Pending, Accepted, Declined, Lapsed },
            [Approval] = new HashSet<ApplicationStatus> { Pending, Accepted, Declined },
            [Processing] = new HashSet<ApplicationStatus> { Pending, Accepted, Declined },
        };

    public static readonly ApplicationState NewApplication = new(DataEntry, Pending);

    public ApplicationPhase Phase { get; }
    public ApplicationStatus Status { get; }

    private ApplicationState(ApplicationPhase phase, ApplicationStatus status)
    {
        Phase = phase;
        Status = status;
    }

    internal static ApplicationState Unsafe(ApplicationPhase phase, ApplicationStatus status)
        => new(phase, status);

    public static Result<ApplicationState> Of(ApplicationPhase phase, ApplicationStatus status)
    {
        if (!_validStatuses.TryGetValue(phase, out var allowed) || !allowed.Contains(status))
            return Result.Failure<ApplicationState>(ApplicationErrors.InvalidState(phase, status));

        return new ApplicationState(phase, status);
    }
}