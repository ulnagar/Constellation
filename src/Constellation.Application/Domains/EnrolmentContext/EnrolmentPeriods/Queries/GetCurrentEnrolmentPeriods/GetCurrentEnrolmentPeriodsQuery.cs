namespace Constellation.Application.Domains.EnrolmentContext.EnrolmentPeriods.Queries.GetCurrentEnrolmentPeriods;

using Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetCurrentEnrolmentPeriodsQuery
    : IQuery<List<EnrolmentPeriodResponse>>;