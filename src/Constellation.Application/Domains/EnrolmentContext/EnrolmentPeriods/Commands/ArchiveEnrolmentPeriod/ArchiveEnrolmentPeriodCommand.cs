namespace Constellation.Application.Domains.EnrolmentContext.EnrolmentPeriods.Commands.ArchiveEnrolmentPeriod;

using Abstractions.Messaging;
using Core.Models.EnrolmentContext.EnrolmentPeriod.Identifiers;

public sealed record ArchiveEnrolmentPeriodCommand(
    EnrolmentPeriodId PeriodId)
    : ICommand;
