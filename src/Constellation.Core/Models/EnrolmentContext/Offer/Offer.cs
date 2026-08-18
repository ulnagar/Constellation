namespace Constellation.Core.Models.EnrolmentContext.Offer;

using Application;
using Application.Enums;
using Application.Identifiers;
using EnrolmentPeriod.Identifiers;
using Enums;
using Errors;
using Events;
using Identifiers;
using Primitives;
using Shared;

public sealed class Offer : AggregateRoot
{
    public static TimeSpan ReminderPeriod => TimeSpan.FromDays(7);
    public static TimeSpan LapsedPeriod => TimeSpan.FromDays(14);

    private static readonly TimeZoneInfo LocalTimeZone =
        TimeZoneInfo.FindSystemTimeZoneById(
            OperatingSystem.IsWindows() ? "AUS Eastern Standard Time" : "Australia/Sydney");

    public Offer()
    {
        Id = new();
        Status = OfferStatus.Processing;
        Response = OfferResponse.Pending;
    }

    public OfferId Id { get; private set; }
    public EnrolmentPeriodId PeriodId { get; private set; } // kept for query convenience
    public ApplicationId ApplicationId { get; private set; }
    public OfferStatus Status { get; private set; }
    public DateTimeOffset? OfferedAt { get; private set; }
    public DateTimeOffset? ReminderSentAt { get; private set; }
    public DateTimeOffset? RespondedAt { get; private set; }

    /// <summary>
    /// The deadline for a parent/carer to respond to a <see cref="OfferResponse.Pending"/> offer.
    /// Always resolves to 5:00pm local school time on the date <see cref="LapsedPeriod"/> days
    /// after <see cref="OfferedAt"/>, rather than preserving the time-of-day component of
    /// <see cref="OfferedAt"/> itself.
    /// </summary>
    public DateTimeOffset? RespondBy
    {
        get
        {
            if (Response != OfferResponse.Pending || RespondedAt is not null)
                return null;

            DateTime localOfferedDate = TimeZoneInfo
                .ConvertTime(OfferedAt!.Value, LocalTimeZone)
                .Date;

            DateTime targetDate = localOfferedDate.Add(LapsedPeriod).Date;

            DateTime unspecified5pm = new(
                targetDate.Year, targetDate.Month, targetDate.Day,
                17, 0, 0, DateTimeKind.Unspecified);

            TimeSpan offset = LocalTimeZone.GetUtcOffset(unspecified5pm);
            return new DateTimeOffset(unspecified5pm, offset);
        }
    }

    public OfferResponse Response { get; private set; }
    public bool HasCourtOrders { get; private set; }
    public bool HasHealthConcerns { get; private set; }
    public bool RequestedLaptop { get; private set; }

    public bool IsReminderDue(DateTimeOffset asOf)
    {
        if (Response != OfferResponse.Pending || ReminderSentAt is not null || RespondBy is null)
            return false;

        DateTime reminderDate = TimeZoneInfo
            .ConvertTime(RespondBy.Value, LocalTimeZone)
            .Date
            .Subtract(ReminderPeriod);

        DateTime asOfLocalDate = TimeZoneInfo
            .ConvertTime(asOf, LocalTimeZone)
            .Date;

        return asOfLocalDate >= reminderDate;
    }

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
        Response = OfferResponse.Pending;

        RaiseDomainEvent(new EnrolmentOfferGeneratedDomainEvent(new(), Id));

        return Result.Success();
    }

    public Result MarkReminderSent(
        DateTimeOffset asOf)
    {
        if (Response != OfferResponse.Pending)
            return Result.Failure(EnrolmentOfferErrors.ReminderInvalid);

        ReminderSentAt = asOf;

        return Result.Success();
    }

    public Result MarkLapsed()
    {
        if (Status != OfferStatus.Pending)
            return Result.Failure(EnrolmentOfferErrors.InvalidStatusChange(Status, OfferStatus.));

        Response = OfferResponse.Lapsed;

        return Result.Success();
    }

    public Result UpdateStatus(OfferStatus newStatus)
    {
        bool isValid = (Status, newStatus) switch
        {
            (OfferStatus.Processing, not OfferStatus.Pending) => false,
            (OfferStatus.Pending, OfferStatus.Processing) => false,
            (OfferResponse.Accepted or OfferResponse.Declined or OfferResponse.Lapsed,
                OfferResponse.Processing or OfferResponse.Pending) => false,
            // Lapsed is only reachable via Lapse() — rejected here defensively
            // in case UpdateStatus is ever called directly with Lapsed.
            (_, OfferResponse.Lapsed) => false,
            _ => true
        };

        if (!isValid)
            return Result.Failure(EnrolmentOfferErrors.InvalidStatusChange(Response, newStatus));

        Response = newStatus;
        return Result.Success();
    }

    public Result Respond(
        OfferResponse status, 
        bool courtOrders = false, 
        bool healthConditions = false,
        bool requestedLaptop = true)
    {
        Result statusUpdate = UpdateStatus(status);

        if (statusUpdate.IsFailure)
            return statusUpdate;

        RespondedAt = DateTime.UtcNow;

        if (status != OfferResponse.Accepted)
            return Result.Success();

        HasCourtOrders = courtOrders;
        HasHealthConcerns = healthConditions;
        RequestedLaptop = requestedLaptop;

        RaiseDomainEvent(new EnrolmentOfferRespondedDomainEvent(new(), Id));

        return Result.Success();
    }
}