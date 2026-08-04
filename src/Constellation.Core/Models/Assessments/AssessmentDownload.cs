namespace Constellation.Core.Models.Assessments;

using Auth;
using Identifiers;

public sealed class AssessmentDownload
{
    private readonly List<AssessmentDownloadEvent> _downloadEvents = [];

    /// <summary>
    /// Required for EF Core
    /// </summary>
    private AssessmentDownload() { }

    public AssessmentDownload(
        AssessmentId assessmentId,
        string name,
        DateOnly availableFrom,
        DateOnly availableTo,
        bool isRestricted)
    {
        Id = new();

        AssessmentId = assessmentId;
        Name = name;
        AvailableFrom = availableFrom;
        AvailableTo = availableTo;
        IsRestricted = isRestricted;
    }

    public AssessmentDownloadId Id { get; init; }
    public AssessmentId AssessmentId { get; private set; }
    public string Name { get; private set; }
    public DateOnly AvailableFrom { get; private set; }
    public DateOnly AvailableTo { get; private set; }
    public bool IsRestricted { get; private set; }
    public bool IsDeleted { get; private set; }

    public IReadOnlyList<AssessmentDownloadEvent> DownloadEvents => _downloadEvents.AsReadOnly();

    public void AddDownloadEvent(AppUser user) => 
        _downloadEvents.Add(new(Id, user));

    public void Delete() => IsDeleted = true;

    public bool IsAvailable(DateOnly today)
    {
        if (IsDeleted)
            return false;
        
        if (today < AvailableFrom)
            return false;

        if (today > AvailableTo)
            return false;

        return true;
    }
}