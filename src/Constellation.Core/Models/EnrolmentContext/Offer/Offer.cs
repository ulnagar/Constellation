namespace Constellation.Core.Models.EnrolmentContext.Offer;

using Application;
using Application.Enums;
using Application.Identifiers;
using EnrolmentPeriod.Identifiers;
using Enums;
using Errors;
using Identifiers;
using Shared;

public sealed class Offer
{
    public static TimeSpan ReminderPeriod => TimeSpan.FromDays(7);
    public static TimeSpan LapsedPeriod => TimeSpan.FromDays(14);

    public Offer()
    {
        Id = new();
        Status = OfferStatus.Processing;
    }

    public OfferId Id { get; private set; }
    public EnrolmentPeriodId PeriodId { get; private set; } // kept for query convenience
    public ApplicationId ApplicationId { get; private set; }
    public OfferStatus Status { get; private set; }
    public DateTimeOffset? OfferedAt { get; private set; }
    public DateTimeOffset? ReminderSentAt { get; private set; }
    public DateTimeOffset? RespondedAt { get; private set; }

    public DateTimeOffset? RespondBy =>
        Status == OfferStatus.Pending && RespondedAt is null
            ? OfferedAt!.Value.Add(LapsedPeriod)
            : null;

    public bool HasCourtOrders { get; private set; }
    public bool HasHealthConcerns { get; private set; }

    public bool IsReminderDue(DateTimeOffset asOf) =>
        Status == OfferStatus.Pending
        && ReminderSentAt is null
        && asOf - OfferedAt!.Value >= ReminderPeriod;

    public static Result<Offer> Create(Application application)
    {
        if (application.Status != ApplicationStatus.Approved)
            return Result.Failure<Offer>(EnrolmentOfferErrors.ApplicationNotApproved);

        Offer offer = new()
        {
            PeriodId = application.PeriodId,
            ApplicationId = application.Id
        };

        return offer;
    }

    public Result MarkOffered(
        DateTimeOffset asOf)
    {
        if (Status != OfferStatus.Processing)
            return Result.Failure(EnrolmentOfferErrors.InvalidStatusChange(Status, OfferStatus.Pending));

        OfferedAt = asOf;
        Status = OfferStatus.Pending;

        return Result.Success();
    }

    public Result MarkReminderSent(
        DateTimeOffset asOf)
    {
        if (Status != OfferStatus.Pending)
            return Result.Failure(EnrolmentOfferErrors.ReminderInvalid);

        ReminderSentAt = asOf;

        return Result.Success();
    }

    public Result MarkLapsed()
    {
        if (Status != OfferStatus.Pending)
            return Result.Failure(EnrolmentOfferErrors.InvalidStatusChange(Status, OfferStatus.Lapsed));

        Status = OfferStatus.Lapsed;

        return Result.Success();
    }

    public Result UpdateStatus(OfferStatus newStatus)
    {
        bool isValid = (Status, newStatus) switch
        {
            (OfferStatus.Processing, not OfferStatus.Pending) => false,
            (OfferStatus.Pending, OfferStatus.Processing) => false,
            (OfferStatus.Accepted or OfferStatus.Declined or OfferStatus.Lapsed,
                OfferStatus.Processing or OfferStatus.Pending) => false,
            // Lapsed is only reachable via Lapse() — rejected here defensively
            // in case UpdateStatus is ever called directly with Lapsed.
            (_, OfferStatus.Lapsed) => false,
            _ => true
        };

        if (!isValid)
            return Result.Failure(EnrolmentOfferErrors.InvalidStatusChange(Status, newStatus));

        Status = newStatus;
        return Result.Success();
    }

    public Result Respond(
        OfferStatus status, 
        bool courtOrders = false, 
        bool healthConditions = false)
    {
        Result statusUpdate = UpdateStatus(status);

        if (statusUpdate.IsFailure)
            return statusUpdate;

        if (status != OfferStatus.Accepted)
            return Result.Success();

        HasCourtOrders = courtOrders;
        HasHealthConcerns = healthConditions;

        return Result.Success();
    }
}