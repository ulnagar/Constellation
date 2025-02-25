namespace Constellation.Application.Attendance.Plans.GetAttendancePlansSummaryForSchool;

using Abstractions.Messaging;
using Core.Models.Attendance;
using Core.Models.Attendance.Enums;
using Core.Models.Attendance.Repositories;
using Core.Shared;
using GetAttendancePlansSummary;
using Serilog;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetAttendancePlansSummaryForSchoolQueryHandler
: IQueryHandler<GetAttendancePlansSummaryForSchoolQuery, List<AttendancePlanSummaryResponse>>
{
    private readonly IAttendancePlanRepository _planRepository;
    private readonly ILogger _logger;

    public GetAttendancePlansSummaryForSchoolQueryHandler(
        IAttendancePlanRepository planRepository,
        ILogger logger)
    {
        _planRepository = planRepository;
        _logger = logger
            .ForContext<GetAttendancePlansSummaryForSchoolQuery>();
    }

    public async Task<Result<List<AttendancePlanSummaryResponse>>> Handle(GetAttendancePlansSummaryForSchoolQuery request, CancellationToken cancellationToken)
    {
        List<AttendancePlanSummaryResponse> response = new();

        List<AttendancePlan> plans = await _planRepository.GetForSchool(request.SchoolCode, cancellationToken);

        foreach (AttendancePlan plan in plans)
        {
            if (plan.Status.Equals(AttendancePlanStatus.Archived))
                continue;

            response.Add(new(
                plan.Id,
                plan.Student,
                plan.Grade,
                plan.School,
                plan.CreatedAt,
                plan.Status,
                0));
        }

        return response;
    }
}
