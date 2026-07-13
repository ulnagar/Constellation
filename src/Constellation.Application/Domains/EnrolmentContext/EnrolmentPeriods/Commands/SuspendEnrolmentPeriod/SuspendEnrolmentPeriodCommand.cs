namespace Constellation.Application.Domains.EnrolmentContext.EnrolmentPeriods.Commands.SuspendEnrolmentPeriod;

using Abstractions.Messaging;
using Constellation.Core.Models.EnrolmentContext.EnrolmentPeriod.Identifiers;

public sealed record SuspendEnrolmentPeriodCommand(
    EnrolmentPeriodId PeriodId,
    string SuspensionComment)
    : ICommand;