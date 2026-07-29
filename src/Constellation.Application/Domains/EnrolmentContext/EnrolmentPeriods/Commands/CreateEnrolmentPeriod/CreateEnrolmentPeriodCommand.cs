namespace Constellation.Application.Domains.EnrolmentContext.EnrolmentPeriods.Commands.CreateEnrolmentPeriod;

using Abstractions.Messaging;
using Core.Models.EnrolmentContext.Application.Enums;
using Core.Models.EnrolmentContext.EnrolmentPeriod.Enums;

public sealed record CreateEnrolmentPeriodCommand(
    string Label,
    string Year,
    DateTimeOffset OpenAt,
    DateTimeOffset ClosedAt,
    Program Program,
    IReadOnlyList<EnrolmentCourse> AvailableCourses)
    : ICommand;
