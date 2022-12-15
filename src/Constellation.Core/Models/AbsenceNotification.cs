namespace Constellation.Core.Models;

public class AbsenceNotification
{
    public const string SMS = "SMS";
    public const string Email = "Email";

    public AbsenceNotification()
    {
        Id = Guid.Empty;
    }

    public Guid Id { get; set; }

    public Guid AbsenceId { get; set; }
    public virtual Absence? Absence { get; set; }

    public string Type { get; set; } = string.Empty;
    public string OutgoingId { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Recipients { get; set; } = string.Empty;
    public bool ConfirmedDelivered { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public string DeliveredMessageIds { get; set; } = string.Empty;
}