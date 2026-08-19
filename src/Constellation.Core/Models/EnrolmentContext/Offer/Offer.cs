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

    private readonly List<OfferNote> _notes = [];

    public static TimeSpan ReminderPeriod => TimeSpan.FromDays(7);
    public static TimeSpan LapsedPeriod => TimeSpan.FromDays(14);

    private static readonly TimeZoneInfo LocalTimeZone =
        TimeZoneInfo.FindSystemTimeZoneById(
            OperatingSystem.IsWindows() ? "AUS Eastern Standard Time" : "Australia/Sydney");

    public Offer()
    {
        Id = new();
        Status = OfferStatus.Preparing;
        Response = ResponseStatus.NoResponse;
    }

    public OfferId Id { get; private set; }
    public EnrolmentPeriodId PeriodId { get; private set; } // kept for query convenience
    public ApplicationId ApplicationId { get; private set; }
    public OfferStatus Status { get; private set; }
    public DateTimeOffset? OfferedAt { get; private set; }
    public DateTimeOffset? ReminderSentAt { get; private set; }
    public DateTimeOffset? RespondedAt { get; private set; }
    public IReadOnlyList<OfferNote> Notes => _notes.AsReadOnly();

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
            if (Status != OfferStatus.AwaitingResponse || RespondedAt is not null)
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

    public ResponseStatus Response { get; private set; }
    public bool HasCourtOrders { get; private set; }
    public bool HasHealthConcerns { get; private set; }
    public bool RequestedLaptop { get; private set; }

    public bool IsReminderDue(DateTimeOffset asOf)
    {
        if (Status != OfferStatus.AwaitingResponse || ReminderSentAt is not null || RespondBy is null)
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
        Result statusChange = UpdateStatus(OfferStatus.AwaitingResponse);

        if (statusChange.IsFailure)
            return statusChange;

        OfferedAt = asOf;
        
        RaiseDomainEvent(new EnrolmentOfferGeneratedDomainEvent(new(), Id));

        return Result.Success();
    }

    public Result MarkReminderSent(
        DateTimeOffset asOf)
    {
        if (Status != OfferStatus.AwaitingResponse)
            return Result.Failure(EnrolmentOfferErrors.ReminderInvalid);

        ReminderSentAt = asOf;

        return Result.Success();
    }

    public Result MarkLapsed() =>
        UpdateStatus(OfferStatus.Lapsed);

    public Result MarkDocumentsCollected() =>
        UpdateStatus(OfferStatus.ReviewingResponse);

    public Result AddReviewNote(
        string note,
        string createdBy)
    {
        Result<OfferNote> offerNote = OfferNote.Create(Id, note, createdBy);

        if (offerNote.IsFailure)
            return offerNote;

        _notes.Add(offerNote.Value);

        return Result.Success();
    }

    public Result MarkReviewComplete() =>
        UpdateStatus(OfferStatus.PendingAcceptance);

    public Result MarkFinalApproval(bool confirmed) =>
        confirmed 
            ? UpdateStatus(OfferStatus.Accepted) 
            : UpdateStatus(OfferStatus.Declined);

    private Result UpdateStatus(OfferStatus newStatus)
    {
        bool isValid = Status switch
        {
            // Terminal states can never transition further.
            var s when s == OfferStatus.Accepted
                       || s == OfferStatus.Declined
                       || s == OfferStatus.Lapsed => false,

            var s when s == OfferStatus.Preparing =>
                newStatus == OfferStatus.AwaitingResponse,

            // CollectingDocuments/ReviewingResponse/Declined branch here is
            // only reachable via RecordResponse(); Declined-via-AwaitingResponse
            // is the parent-decline path.
            var s when s == OfferStatus.AwaitingResponse =>
                newStatus == OfferStatus.CollectingDocuments
                || newStatus == OfferStatus.ReviewingResponse
                || newStatus == OfferStatus.Declined
                || newStatus == OfferStatus.Lapsed,

            var s when s == OfferStatus.CollectingDocuments =>
                newStatus == OfferStatus.ReviewingResponse,

            var s when s == OfferStatus.ReviewingResponse =>
                newStatus == OfferStatus.PendingAcceptance,

            // Accepted/Declined here is only reachable via FinalisePrincipalDecision().
            var s when s == OfferStatus.PendingAcceptance =>
                newStatus == OfferStatus.Accepted || newStatus == OfferStatus.Declined,

            _ => false
        };

        if (!isValid)
            return Result.Failure(EnrolmentOfferErrors.InvalidStatusChange(Status, newStatus));

        Status = newStatus;
        return Result.Success();
    }

    public Result Respond(
        ResponseStatus response, 
        bool courtOrders = false, 
        bool healthConditions = false,
        bool requestedLaptop = true)
    {
        OfferStatus newStatus = OfferStatus.Declined;

        if (response == ResponseStatus.Accepted)
        {
            if (courtOrders || healthConditions)
                newStatus = OfferStatus.CollectingDocuments;
            else
                newStatus = OfferStatus.PendingAcceptance;
        }

        Result statusUpdate = UpdateStatus(newStatus);

        if (statusUpdate.IsFailure)
            return statusUpdate;

        RespondedAt = DateTime.UtcNow;
        Response = response;

        if (Response != ResponseStatus.Accepted)
            return Result.Success();

        HasCourtOrders = courtOrders;
        HasHealthConcerns = healthConditions;
        RequestedLaptop = requestedLaptop;

        RaiseDomainEvent(new EnrolmentOfferRespondedDomainEvent(new(), Id));

        return Result.Success();
    }
}