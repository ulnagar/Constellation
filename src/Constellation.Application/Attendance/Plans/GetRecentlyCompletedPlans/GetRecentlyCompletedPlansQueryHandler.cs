namespace Constellation.Application.Attendance.Plans.GetRecentlyCompletedPlans;

using Abstractions.Messaging;
using Core.Models.Attendance;
using Core.Models.Attendance.Repositories;
using Core.Shared;
using Serilog;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetRecentlyCompletedPlansQueryHandler
: IQueryHandler<GetRecentlyCompletedPlansQuery, List<CompletedPlansResponse>>
{
    private readonly IAttendancePlanRepository _planRepository;
    private readonly ILogger _logger;

    public GetRecentlyCompletedPlansQueryHandler(
        IAttendancePlanRepository planRepository,
        ILogger logger)
    {
        _planRepository = planRepository;
        _logger = logger
            .ForContext<GetRecentlyCompletedPlansQuery>();
    }
    public async Task<Result<List<CompletedPlansResponse>>> Handle(GetRecentlyCompletedPlansQuery request, CancellationToken cancellationToken)
    {
        List<CompletedPlansResponse> response = new();
        
        List<AttendancePlan> plans = await _planRepository.GetRecentForSchoolAndGrade(request.SchoolCode, request.Grade, cancellationToken);

        foreach (AttendancePlan plan in plans)
        {
            response.Add(new(
                plan.Id,
                plan.Student));
        }

        return response;
    }
}
