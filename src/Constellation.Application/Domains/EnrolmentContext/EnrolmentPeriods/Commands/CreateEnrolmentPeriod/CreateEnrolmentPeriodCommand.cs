namespace Constellation.Application.Domains.EnrolmentContext.EnrolmentPeriods.Commands.CreateEnrolmentPeriod;

using Abstractions.Messaging;
using Core.Models.EnrolmentContext.Offer.Enums;

public sealed record CreateEnrolmentPeriodCommand(
    string Label,
    DateTimeOffset OpenAt,
    DateTimeOffset ClosedAt,
    Program Program)
    : ICommand;
