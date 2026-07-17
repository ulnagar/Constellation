namespace Constellation.Core.Models.EnrolmentContext.Offer;

using Application;
using Application.Identifiers;
using EnrolmentPeriod.Identifiers;
using Enums;
using Identifiers;

public sealed class Offer
{
    public Offer()
    {
        Id = new();
        Status = OfferStatus.Pending;
        OfferedAt = DateTimeOffset.Now;
    }

    public OfferId Id { get; private set; }
    public EnrolmentPeriodId PeriodId { get; private set; } // kept for query convenience
    public ApplicationId ApplicationId { get; private set; }
    public OfferStatus Status { get; private set; }
    public DateTimeOffset OfferedAt { get; private set; }
    public DateTimeOffset RespondBy { get; private set; }
    public DateTimeOffset? ReminderSentAt { get; private set; }
    public DateTimeOffset? RespondedAt { get; private set; }

    public bool HasCourtOrders { get; private set; }
    public bool HasHealthConcerns { get; private set; }

    public static Offer Create(Application application)
    {
        Offer offer = new()
        {
            PeriodId = application.PeriodId,
            ApplicationId = application.Id
        };

        return offer;
    }
}