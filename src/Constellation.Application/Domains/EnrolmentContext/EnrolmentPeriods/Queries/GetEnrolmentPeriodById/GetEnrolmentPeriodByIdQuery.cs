namespace Constellation.Application.Domains.EnrolmentContext.EnrolmentPeriods.Queries.GetEnrolmentPeriodById;

using Abstractions.Messaging;
using Core.Models.EnrolmentContext.EnrolmentPeriod.Identifiers;
using Models;

public sealed record GetEnrolmentPeriodByIdQuery(
    EnrolmentPeriodId Id)
    : IQuery<EnrolmentPeriodResponse>;