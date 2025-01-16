namespace Constellation.Application.Attendance.Plans.GetRecentlyCompletedPlans;

using Abstractions.Messaging;
using Core.Enums;
using System.Collections.Generic;

public sealed record GetRecentlyCompletedPlansQuery(
    string SchoolCode,
    Grade Grade)
    : IQuery<List<CompletedPlansResponse>>;