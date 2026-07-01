namespace Constellation.Application.Domains.EnrolmentContext.Applications.Queries.GetEnrolmentApplicationsByPeriod;

using Abstractions.Messaging;
using Core.Models.EnrolmentContext.EnrolmentPeriod.Identifiers;
using Models;
using System.Collections.Generic;

public sealed record GetEnrolmentApplicationsByPeriodQuery(
    EnrolmentPeriodId PeriodId)
    : IQuery<List<EnrolmentApplicationResponse>>;