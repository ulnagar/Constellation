namespace Constellation.Application.Attendance.Plans.GetAttendancePlansSummary;

using Abstractions.Messaging;
using Core.Models.Attendance;
using Core.Models.Attendance.Enums;
using Core.Models.Attendance.Repositories;
using Core.Shared;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetAttendancePlansSummaryQueryHandler
: IQueryHandler<GetAttendancePlansSummaryQuery, List<AttendancePlanSummaryResponse>>
{
    private readonly IAttendancePlanRepository _planRepository;
    private readonly ILogger _logger;

    public GetAttendancePlansSummaryQueryHandler(
        IAttendancePlanRepository planRepository,
        ILogger logger)
    {
        _planRepository = planRepository;
        _logger = logger;
    }

    public async Task<Result<List<AttendancePlanSummaryResponse>>> Handle(GetAttendancePlansSummaryQuery request, CancellationToken cancellationToken)
    {
        List<AttendancePlanSummaryResponse> response = new();

        List<AttendancePlan> plans = await _planRepository.GetAll(cancellationToken);

        foreach (AttendancePlan plan in plans)
        {
            if (plan.Status.Equals(AttendancePlanStatus.Archived))
                continue;

            double overallPercentage = (plan.Periods.Sum(period => period.MinutesPresent)) / (plan.Periods.DistinctBy(period => period.CourseId).Sum(period => period.TargetMinutesPerCycle));

            if (overallPercentage is Double.NaN)
                overallPercentage = Double.PositiveInfinity;

            response.Add(new(
                plan.Id,
                plan.Student,
                plan.Grade,
                plan.School,
                plan.CreatedAt,
                plan.Status,
                overallPercentage));
        }

        return response;
    }
}
