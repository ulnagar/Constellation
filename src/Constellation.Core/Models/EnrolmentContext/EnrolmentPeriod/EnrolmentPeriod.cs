namespace Constellation.Core.Models.EnrolmentContext.EnrolmentPeriod;

using Enums;
using Errors;
using Identifiers;
using Offer.Enums;
using Shared;
using System;

public sealed class EnrolmentPeriod
{
    /// <summary>
    /// DO NOT USE. EF Core only.
    /// </summary>
    private EnrolmentPeriod() { }

    private EnrolmentPeriod(
        string label,
        DateTimeOffset openAt,
        DateTimeOffset closeAt,
        Program program)
    {
        Id = new();

        Label = label;
        OpenAt = openAt;
        ClosedAt = closeAt;
        Program = program;
        Status = PeriodStatus.Scheduled;
    }

    public EnrolmentPeriodId Id { get; private set; }
    public string Label { get; private set; }
    public DateTimeOffset OpenAt { get; private set; }
    public DateTimeOffset ClosedAt { get; private set; }
    public PeriodStatus Status { get; private set; }
    public Program Program { get; private set; }

    public static Result<EnrolmentPeriod> Create(
        string label,
        DateTimeOffset openAt,
        DateTimeOffset closeAt,
        Program program)
    {
        Result validationResult = ValidatePeriod(openAt, closeAt, DateTimeOffset.UtcNow);

        if (validationResult.IsFailure)
            return Result.Failure<EnrolmentPeriod>(validationResult.Error);

        return new EnrolmentPeriod(
            label,
            openAt,
            closeAt,
            program);
    }

    public Result Update(
        string label,
        DateTimeOffset openAt,
        DateTimeOffset closeAt,
        Program program)
    {
        Result validationResult = ValidatePeriod(openAt, closeAt, DateTimeOffset.UtcNow);

        if (validationResult.IsFailure)
            return validationResult;

        Label = label;
        OpenAt = openAt;
        ClosedAt = closeAt;
        Program = program;

        return Result.Success();
    }

    public Result UpdateStatus(
        PeriodStatus newStatus)
    {
        if (Status == PeriodStatus.Archived)
            return Result.Failure(EnrolmentPeriodErrors.CannotUpdateStatusOfArchivedPeriod);

        Status = newStatus;

        return Result.Success();
    }

    private static Result ValidatePeriod(DateTimeOffset openAt, DateTimeOffset closedAt, DateTimeOffset now)
    {
        TimeSpan minimumDuration = TimeSpan.FromHours(24);
        TimeSpan maximumDuration = TimeSpan.FromDays(365);

        if (openAt == default)
            return Result.Failure(EnrolmentPeriodErrors.OpenAtRequired);

        if (closedAt == default)
            return Result.Failure(EnrolmentPeriodErrors.ClosedAtRequired);

        if (closedAt <= openAt)
            return Result.Failure(EnrolmentPeriodErrors.CloseBeforeOpen);

        if (openAt < now.AddDays(-30))
            return Result.Failure(EnrolmentPeriodErrors.OpenTooFarInPast);

        if (closedAt > now.AddYears(1))
            return Result.Failure(EnrolmentPeriodErrors.CloseTooFarInFuture);

        TimeSpan duration = closedAt - openAt;

        if (duration < minimumDuration)
            return Result.Failure(EnrolmentPeriodErrors.DurationTooShort);

        if (duration > maximumDuration)
            return Result.Failure(EnrolmentPeriodErrors.DurationTooLong);

        return Result.Success();
    }
}