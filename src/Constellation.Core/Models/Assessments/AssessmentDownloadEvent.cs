namespace Constellation.Core.Models.Assessments;

using Auth;
using Core.ValueObjects;
using Identifiers;
using Shared;

public sealed class AssessmentDownloadEvent
{
    internal AssessmentDownloadEvent(
        AssessmentDownloadId downloadId,
        AppUser user)
    {
        DownloadId = downloadId;
        UserId = user.Id;
        DownloadedAt = DateTimeOffset.UtcNow;
        DownloadedBy = user.Name;
        Result<EmailAddress> email = EmailAddress.Create(user.Email);

        if (email.IsSuccess)
            DownloadedByEmail = email.Value;
        else
            DownloadedByEmail = EmailAddress.None;
    }

    public AssessmentDownloadId DownloadId { get; private set; }
    public Guid UserId { get; private set; }
    public DateTimeOffset DownloadedAt { get; private set; }

    public string DownloadedBy { get; private set; }
    public EmailAddress DownloadedByEmail { get; private set; }
}