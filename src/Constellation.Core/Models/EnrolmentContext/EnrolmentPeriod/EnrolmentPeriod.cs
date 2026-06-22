namespace Constellation.Core.Models.EnrolmentContext.EnrolmentPeriod;

using Enums;
using Identifiers;
using System;
using System.Collections.Generic;
using System.Text;

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

}