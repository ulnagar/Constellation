namespace Constellation.Application.Domains.Attendance.Plans.Commands.CopyAttendancePlanDetails;

using Abstractions.Messaging;
using Core.Models.Attendance;
using Core.Models.Attendance.Errors;
using Core.Models.Attendance.Repositories;
using Core.Models.Timetables.Enums;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CopyAttendancePlanDetailsCommandHandler
: ICommandHandler<CopyAttendancePlanDetailsCommand>
{
    private readonly IAttendancePlanRepository _planRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public CopyAttendancePlanDetailsCommandHandler(
        IAttendancePlanRepository planRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _planRepository = planRepository;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<CopyAttendancePlanDetailsCommand>();
    }

    public async Task<Result> Handle(CopyAttendancePlanDetailsCommand request, CancellationToken cancellationToken)
    {
        AttendancePlan sourcePlan = await _planRepository.GetById(request.SourcePlanId, cancellationToken);

        if (sourcePlan is null)
        {
            _logger
                .ForContext(nameof(CopyAttendancePlanDetailsCommand), request, true)
                .ForContext(nameof(Error), AttendancePlanErrors.NotFound(request.SourcePlanId), true)
                .Warning("Failed to retrieve Source Attendance Plan for copy");

            return Result.Failure(AttendancePlanErrors.NotFound(request.SourcePlanId));
        }

        AttendancePlan destinationPlan = await _planRepository.GetById(request.DestinationPlanId, cancellationToken);

        if (destinationPlan is null)
        {
            _logger
                .ForContext(nameof(CopyAttendancePlanDetailsCommand), request, true)
                .ForContext(nameof(Error), AttendancePlanErrors.NotFound(request.DestinationPlanId), true)
                .Warning("Failed to retrieve Destination Attendance Plan for copy");

            return Result.Failure(AttendancePlanErrors.NotFound(request.DestinationPlanId));
        }

        foreach (var period in destinationPlan.Periods)
        {
            AttendancePlanPeriod sourcePeriod = sourcePlan.Periods.FirstOrDefault(entry => entry.PeriodId == period.PeriodId);

            if (sourcePeriod is null)
                continue;

            destinationPlan.CopyPeriodValues(
                period.Id, 
                sourcePeriod.EntryTime, 
                sourcePeriod.ExitTime);
        }

        if (sourcePlan.SciencePracLesson is not null)
        {
            destinationPlan.UpdateSciencePracLesson(
                sourcePlan.SciencePracLesson.Week,
                sourcePlan.SciencePracLesson.Day,
                sourcePlan.SciencePracLesson.Period);
        }

        List<(string, double, double)> missedLessons = sourcePlan.MissedLessons
            .Select(entry => (entry.Subject, entry.TotalMinutesPerCycle, entry.MinutesMissedPerCycle))
            .ToList();

        destinationPlan.AddMissedLessons(missedLessons);

        List<(PeriodWeek Week, PeriodDay Day, string Period, double Minutes, string Activity)> freePeriods = sourcePlan.FreePeriods
            .Select(entry => (entry.Week, entry.Day, entry.Period, entry.Minutes, entry.Activity))
            .ToList();

        destinationPlan.AddFreePeriods(freePeriods);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
