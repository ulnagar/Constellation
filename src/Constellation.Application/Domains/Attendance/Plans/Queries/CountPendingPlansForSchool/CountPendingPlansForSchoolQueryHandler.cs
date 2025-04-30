namespace Constellation.Application.Domains.Attendance.Plans.Queries.CountPendingPlansForSchool;

using Abstractions.Messaging;
using Core.Models.Attendance;
using Core.Models.Attendance.Repositories;
using Core.Shared;
using Serilog;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CountPendingPlansForSchoolQueryHandler
: IQueryHandler<CountPendingPlansForSchoolQuery, int>
{
    private readonly IAttendancePlanRepository _planRepository;
    private readonly ILogger _logger;

    public CountPendingPlansForSchoolQueryHandler(
        IAttendancePlanRepository planRepository,
        ILogger logger)
    {
        _planRepository = planRepository;
        _logger = logger
            .ForContext<CountPendingPlansForSchoolQuery>();
    }

    public async Task<Result<int>> Handle(CountPendingPlansForSchoolQuery request, CancellationToken cancellationToken)
    {
        List<AttendancePlan> plans = await _planRepository.GetPendingForSchool(request.SchoolCode, cancellationToken);

        return plans.Count;
    }
}
