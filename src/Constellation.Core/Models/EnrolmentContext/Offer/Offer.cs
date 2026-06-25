namespace Constellation.Core.Models.EnrolmentContext.Offer;

using Application.Identifiers;
using EnrolmentPeriod.Identifiers;
using Identifiers;
using Status = Enums.Status;

public sealed class Offer
{
    public Offer()
    {
        Id = new();
    }

    public OfferId Id { get; private set; }
    public EnrolmentPeriodId PeriodId { get; private set; } // kept for query convenience
    public ApplicationId ApplicationId { get; private set; }
    public Status Status { get; private set; }
    public DateTimeOffset OfferedAt { get; private set; }
    public DateTimeOffset RespondBy { get; private set; }
    public DateTimeOffset? ReminderSentAt { get; private set; }
    public DateTimeOffset? RespondedAt { get; private set; }
}