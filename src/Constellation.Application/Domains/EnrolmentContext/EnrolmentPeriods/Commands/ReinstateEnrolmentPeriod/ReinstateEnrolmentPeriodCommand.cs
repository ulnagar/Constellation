namespace Constellation.Application.Domains.EnrolmentContext.EnrolmentPeriods.Commands.ReinstateEnrolmentPeriod;

using Abstractions.Messaging;
using Core.Models.EnrolmentContext.EnrolmentPeriod.Identifiers;

public sealed record ReinstateEnrolmentPeriodCommand(
    EnrolmentPeriodId PeriodId)
    : ICommand;