namespace Constellation.Application.Attendance.Plans.GetAttendancePlanForSubmit;

using Abstractions.Messaging;
using Core.Models.Attendance;
using Core.Models.Attendance.Errors;
using Core.Models.Attendance.Repositories;
using Core.Shared;
using Serilog;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetAttendancePlanForSubmitQueryHandler
: IQueryHandler<GetAttendancePlanForSubmitQuery, AttendancePlanEntry>
{
    private readonly IAttendancePlanRepository _planRepository;
    private readonly ILogger _logger;

    public GetAttendancePlanForSubmitQueryHandler(
        IAttendancePlanRepository planRepository,
        ILogger logger)
    {
        _planRepository = planRepository;
        _logger = logger
            .ForContext<GetAttendancePlanForSubmitQuery>();
    }

    public async Task<Result<AttendancePlanEntry>> Handle(GetAttendancePlanForSubmitQuery request, CancellationToken cancellationToken)
    {
        AttendancePlan plan = await _planRepository.GetById(request.PlanId, cancellationToken);

        if (plan is null)
        {
            _logger
                .ForContext(nameof(GetAttendancePlanForSubmitQuery), request, true)
                .ForContext(nameof(Error), AttendancePlanErrors.NotFound(request.PlanId), true)
                .Warning("Failed to retrieve attendance plan");

            return Result.Failure<AttendancePlanEntry>(AttendancePlanErrors.NotFound(request.PlanId));
        }

        List<AttendancePlanEntry.PlanPeriod> periods = new();

        foreach (AttendancePlanPeriod period in plan.Periods)
        {
            periods.Add(new(
                period.Id,
                period.Timetable,
                period.Week,
                period.Day,
                period.PeriodName,
                period.PeriodType,
                period.StartTime,
                period.EndTime,
                period.OfferingName,
                period.CourseName,
                period.EntryTime,
                period.ExitTime));
        }

        AttendancePlanEntry response = new(
            plan.Id,
            plan.Status,
            plan.StudentId,
            plan.Student,
            plan.Grade,
            plan.SchoolCode,
            plan.School,
            periods);

        return response;
    }
}
