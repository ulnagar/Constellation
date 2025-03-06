namespace Constellation.Application.Attendance.Plans.CreateAttendancePlanVersion;

using Abstractions.Messaging;
using Constellation.Core.Enums;
using Constellation.Core.Models.Attendance.Enums;
using Constellation.Core.Models.Attendance.Errors;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Subjects;
using Constellation.Core.Models.Subjects.Repositories;
using Constellation.Core.Models.Timetables;
using Constellation.Core.Models.Timetables.Identifiers;
using Constellation.Core.Models.Timetables.Repositories;
using Core.Abstractions.Clock;
using Core.Models.Attendance;
using Core.Models.Attendance.Identifiers;
using Core.Models.Attendance.Repositories;
using Core.Models.Offerings;
using Core.Models.Students.Errors;
using Core.Models.Students.Repositories;
using Core.Models.Timetables.Enums;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;


internal sealed class CreateAttendancePlanVersionCommandHandler
: ICommandHandler<CreateAttendancePlanVersionCommand, AttendancePlanId>
{
    private readonly IAttendancePlanRepository _planRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IPeriodRepository _periodRepository;
    private readonly IDateTimeProvider _dateTime;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public CreateAttendancePlanVersionCommandHandler(
        IAttendancePlanRepository planRepository,
        IStudentRepository studentRepository,
        IOfferingRepository offeringRepository,
        ICourseRepository courseRepository,
        IPeriodRepository periodRepository,
        IDateTimeProvider dateTime,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _planRepository = planRepository;
        _studentRepository = studentRepository;
        _offeringRepository = offeringRepository;
        _courseRepository = courseRepository;
        _periodRepository = periodRepository;
        _dateTime = dateTime;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<AttendancePlanId>> Handle(CreateAttendancePlanVersionCommand request, CancellationToken cancellationToken)
    {
        AttendancePlan currentPlan = await _planRepository.GetById(request.PlanId, cancellationToken);

        if (currentPlan is null)
        {
            _logger
                .ForContext(nameof(CreateAttendancePlanVersionCommand), request, true)
                .ForContext(nameof(Error), AttendancePlanErrors.NotFound(request.PlanId), true)
                .Warning("Failed to create version of Accepted Attendance Plan");

            return Result.Failure<AttendancePlanId>(AttendancePlanErrors.NotFound(request.PlanId));
        }

        if (currentPlan.Status != AttendancePlanStatus.Accepted)
        {
            _logger
                .ForContext(nameof(CreateAttendancePlanVersionCommand), request, true)
            .ForContext(nameof(Error), AttendancePlanErrors.StatusNotAccepted, true)
            .Warning("Failed to create version of Accepted Attendance Plan");

            return Result.Failure<AttendancePlanId>(AttendancePlanErrors.StatusNotAccepted);
        }

        Student student = await _studentRepository.GetById(currentPlan.StudentId, cancellationToken);

        if (student is null)
        {
            _logger
                .ForContext(nameof(CreateAttendancePlanVersionCommand), request, true)
                .ForContext(nameof(Error), StudentErrors.NotFound(currentPlan.StudentId), true)
                .Warning("Failed to create version of Accepted Attendance Plan");

            return Result.Failure<AttendancePlanId>(StudentErrors.NotFound(currentPlan.StudentId));
        }

        // Check for existing Pending or Processing plan for the student
        List<AttendancePlan> studentPlans = await _planRepository.GetForStudent(currentPlan.StudentId, cancellationToken);
        List<AttendancePlan> inProgressPlans = studentPlans
        .Where(entry =>
                entry.CreatedAt.Year == _dateTime.CurrentYear &&
                (entry.Status == AttendancePlanStatus.Pending || entry.Status == AttendancePlanStatus.Processing))
            .ToList();

        if (inProgressPlans.Count > 0)
        {
            _logger
                .ForContext(nameof(CreateAttendancePlanVersionCommand), request, true)
                .ForContext(nameof(Error), AttendancePlanErrors.ExistingInProgressPlanFound, true)
                .Warning("Failed to create version of Accepted Attendance Plan");

            return Result.Failure<AttendancePlanId>(AttendancePlanErrors.ExistingInProgressPlanFound);
        }

        // Generate new plan
        AttendancePlan newPlan = AttendancePlan.Create(student);

        List<Offering> offerings = await _offeringRepository.GetByStudentId(student.Id, cancellationToken);

        foreach (Offering offering in offerings)
        {
            Course course = await _courseRepository.GetByOfferingId(offering.Id, cancellationToken);

            if (course is null)
                continue;

            // Skip all Tutorial courses
            if (course.Code == "TUT")
                continue;

            // Skip all courses from Stage 6
            if (course.Grade is Grade.Y11 or Grade.Y12)
                continue;

            List<PeriodId> periodIds = offering.Sessions
                .Where(session => !session.IsDeleted)
                .Select(session => session.PeriodId)
                .ToList();

            List<Period> periods = await _periodRepository.GetListFromIds(periodIds, cancellationToken);

            newPlan.AddPeriods(periods, offering, course);
        }

        // Verify the plan has some periods attached
        if (newPlan.Periods.Count == 0)
        {
            _logger
                .ForContext(nameof(CreateAttendancePlanVersionCommand), request, true)
                .ForContext(nameof(Error), AttendancePlanErrors.NoPeriodsFound, true)
                .Warning("Failed to create version of Accepted Attendance Plan");

            return Result.Failure<AttendancePlanId>(StudentErrors.NotFound(currentPlan.StudentId));
        }

        // Copy values from current plan to new plan
        foreach (var period in newPlan.Periods)
        {
            AttendancePlanPeriod sourcePeriod = currentPlan.Periods.FirstOrDefault(entry => entry.PeriodId == period.PeriodId);

            if (sourcePeriod is null)
                continue;

            newPlan.CopyPeriodValues(
                period.Id,
                sourcePeriod.EntryTime,
                sourcePeriod.ExitTime);
        }

        if (currentPlan.SciencePracLesson is not null)
        {
            newPlan.UpdateSciencePracLesson(
                currentPlan.SciencePracLesson.Week,
                currentPlan.SciencePracLesson.Day,
                currentPlan.SciencePracLesson.Period);
        }

        List<(string, double, double)> missedLessons = currentPlan.MissedLessons
            .Select(entry => (entry.Subject, entry.TotalMinutesPerCycle, entry.MinutesMissedPerCycle))
            .ToList();

        newPlan.AddMissedLessons(missedLessons);

        List<(PeriodWeek Week, PeriodDay Day, string Period, double Minutes, string Activity)> freePeriods = currentPlan.FreePeriods
            .Select(entry => (entry.Week, entry.Day, entry.Period, entry.Minutes, entry.Activity))
            .ToList();

        newPlan.AddFreePeriods(freePeriods);

        _planRepository.Insert(newPlan);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return newPlan.Id;
    }
}