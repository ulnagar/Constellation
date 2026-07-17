namespace Constellation.Application.Domains.EnrolmentContext.EnrolmentPeriods.Commands.UpdateEnrolmentPeriod;

using Abstractions.Messaging;
using Core.Models.EnrolmentContext.EnrolmentPeriod.Enums;
using Core.Models.EnrolmentContext.EnrolmentPeriod.Identifiers;
using Core.Models.EnrolmentContext.Offer.Enums;
using System;

public sealed record UpdateEnrolmentPeriodCommand(
    EnrolmentPeriodId Id,
    string Label,
    DateTimeOffset OpenAt,
    DateTimeOffset ClosedAt,
    Program Program)
    : ICommand;
