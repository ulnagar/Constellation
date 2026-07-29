namespace Constellation.Application.Domains.EnrolmentContext.EnrolmentPeriods.Commands.UpdateEnrolmentPeriod;

using Abstractions.Messaging;
using Core.Models.EnrolmentContext.Application.Enums;
using Core.Models.EnrolmentContext.EnrolmentPeriod.Enums;
using Core.Models.EnrolmentContext.EnrolmentPeriod.Identifiers;
using System;

public sealed record UpdateEnrolmentPeriodCommand(
    EnrolmentPeriodId Id,
    string Label,
    string Year,
    DateTimeOffset OpenAt,
    DateTimeOffset ClosedAt,
    Program Program,
    IReadOnlyList<EnrolmentCourse> AvailableCourses)
    : ICommand;
