namespace Constellation.Application.Domains.EnrolmentContext.EnrolmentPeriods.Queries.GetAllEnrolmentPeriods;

using Abstractions.Messaging;
using Models;
using System.Collections.Generic;

public sealed record GetAllEnrolmentPeriodsQuery()
    : IQuery<List<EnrolmentPeriodResponse>>;
