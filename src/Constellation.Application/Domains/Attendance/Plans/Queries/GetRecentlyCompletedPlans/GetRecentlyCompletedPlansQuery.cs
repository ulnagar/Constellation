namespace Constellation.Application.Domains.Attendance.Plans.Queries.GetRecentlyCompletedPlans;

using Abstractions.Messaging;
using Core.Enums;
using Core.Models.Identifiers;
using System.Collections.Generic;

public sealed record GetRecentlyCompletedPlansQuery(
    SchoolCode SchoolCode,
    Grade Grade)
    : IQuery<List<CompletedPlansResponse>>;