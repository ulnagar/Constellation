namespace Constellation.Core.Models.EnrolmentContext.EnrolmentPeriod;

using Enums;
using Identifiers;
using Offer.Enums;
using System;

public sealed class EnrolmentPeriod
{
    public EnrolmentPeriod()
    {
        Id = new();
    }

    public EnrolmentPeriodId Id { get; private set; }
    public string Label { get; private set; }
    public DateTimeOffset OpenAt { get; private set; }
    public DateTimeOffset ClosedAt { get; private set; }
    public PeriodStatus Status { get; private set; }
    public Program Program { get; private set; }
}