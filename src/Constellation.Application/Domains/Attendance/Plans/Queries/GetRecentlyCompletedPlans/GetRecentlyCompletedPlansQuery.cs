namespace Constellation.Application.Domains.Attendance.Plans.Queries.GetRecentlyCompletedPlans;

using Abstractions.Messaging;
using Core.Enums;
using System.Collections.Generic;

public sealed record GetRecentlyCompletedPlansQuery(
    string SchoolCode,
    Grade Grade)
    : IQuery<List<CompletedPlansResponse>>;