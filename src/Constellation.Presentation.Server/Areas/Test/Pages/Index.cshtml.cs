namespace Constellation.Presentation.Server.Areas.Test.Pages;

using Application.Interfaces.Repositories;
using BaseModels;
using Core.Abstractions.Clock;
using Core.Abstractions.Services;
using Core.Models.Attendance;
using Core.Models.Attendance.Enums;
using Core.Models.Attendance.Identifiers;
using Core.Models.Attendance.Repositories;
using Core.Models.Offerings.Identifiers;
using Core.Models.Offerings.Repositories;
using Core.Models.Students.Repositories;
using Core.Models.Students.ValueObjects;
using Core.Models.Subjects.Identifiers;
using Core.Models.Subjects.Repositories;
using Core.Models.Timetables.Enums;
using Core.Models.Timetables.Repositories;
using Core.Shared;
using MediatR;
using Serilog;

public class IndexModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly IStudentRepository _studentRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IOfferingRepository _offeringRepository;
    private readonly IPeriodRepository _periodRepository;
    private readonly IAttendancePlanRepository _attendancePlanRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger _logger;

    public IndexModel(
        IMediator mediator,
        IStudentRepository studentRepository,
        ICourseRepository courseRepository,
        IOfferingRepository offeringRepository,
        IPeriodRepository periodRepository,
        IAttendancePlanRepository attendancePlanRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTime,
        ILogger logger)
    {
        _mediator = mediator;
        _studentRepository = studentRepository;
        _courseRepository = courseRepository;
        _offeringRepository = offeringRepository;
        _periodRepository = periodRepository;
        _attendancePlanRepository = attendancePlanRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _dateTime = dateTime;
        _logger = logger;
    }

    public List<AttendancePlan> Plans { get; set; }
    public List<ProjectedPercentage> Percentages { get; set; } = new();

    public async Task OnGet()
    {
        await SavePlansToDb();

        Plans = await _attendancePlanRepository.GetAll();

        foreach (var plan in Plans)
            await ExtendPlans(plan);
    }

    private async Task SavePlansToDb()
    {
        AttendancePlan? leo = await LeoWoolacott();

        if (leo is not null)
            _attendancePlanRepository.Insert(leo);

        AttendancePlan? mackenzie = await MackenzieJohnson();

        if (mackenzie is not null)
            _attendancePlanRepository.Insert(mackenzie);

        AttendancePlan? lincoln = await LincolnPackham();

        if (lincoln is not null)
            _attendancePlanRepository.Insert(lincoln);

        await _unitOfWork.CompleteAsync();
    }

    private async Task ExtendPlans(AttendancePlan plan)
    {
        if (plan.Status == AttendancePlanStatus.Pending)
            return;

        var offeringId = plan.Periods.First().OfferingId;

        var offerings = await _offeringRepository.GetOfferingsFromSameGroup(offeringId);

        foreach (var offering in offerings.OrderBy(offering => offering.Name))
        {
            var course = await _courseRepository.GetById(offering.CourseId);

            var periodIds = offering.Sessions.Where(session => !session.IsDeleted).Select(session => session.PeriodId);

            double total = 0;

            foreach (var periodId in periodIds)
            {
                var matchingPeriod = plan.Periods.SingleOrDefault(period => period.PeriodId == periodId);

                if (matchingPeriod is null)
                    continue;

                total += matchingPeriod.MinutesPresent;
            }

            Percentages.Add(new()
            {
                PlanId = plan.Id,
                CourseId = course.Id,
                Course = course.Name,
                OfferingId = offering.Id,
                Class = offering.Name,
                MinutesPresent = total,
                Percentage = (total / course.TargetMinutesPerCycle)
            });
        }
    }

    private async Task<AttendancePlan?> LeoWoolacott()
    {
        var srn = StudentReferenceNumber.Create("451054805");

        var student = await _studentRepository.GetBySRN(srn.Value);

        var attendancePlan = AttendancePlan.Create(student);

        var offerings = await _offeringRepository.GetByStudentId(student.Id);

        foreach (var offering in offerings)
        {
            var course = await _courseRepository.GetByOfferingId(offering.Id);

            var periodIds = offering.Sessions
                .Where(session => !session.IsDeleted)
                .Select(session => session.PeriodId)
                .ToList();

            var periods = await _periodRepository.GetListFromIds(periodIds);

            attendancePlan.AddPeriods(periods, offering, course);
        }

        //List<(AttendancePlanPeriodId PeriodId, TimeOnly EntryTime, TimeOnly ExitTime)> periodUpdates = new();

        //foreach (var period in attendancePlan.Periods)
        //{
        //    if (period.StartTime == new TimeOnly(9, 10, 0))
        //    {
        //    }

        //    if (period.StartTime == new TimeOnly(11, 30, 0))
        //    {
        //        periodUpdates.Add(new()
        //        {
        //            PeriodId = period.Id, 
        //            EntryTime = new(11, 50, 0), 
        //            ExitTime = new(13, 10, 0)
        //        });
        //    }

        //    if (period.StartTime == new TimeOnly(14, 10, 0))
        //    {
        //        periodUpdates.Add(new()
        //        {
        //            PeriodId = period.Id,
        //            EntryTime = new(14, 10, 0),
        //            ExitTime = new(15, 00, 0)
        //        });
        //    }
        //}

        //Result periodUpdate = attendancePlan.UpdatePeriods(periodUpdates, _currentUserService, _dateTime);

        //if (periodUpdate.IsFailure)
        //{
        //    _logger
        //        .ForContext(nameof(Error), periodUpdate.Error, true)
        //        .Warning("Failed to update Attendance Plan Period with supplied values");

        //    return null;
        //}

        return attendancePlan;
    }

    private async Task<AttendancePlan?> MackenzieJohnson()
    {
        var srn = StudentReferenceNumber.Create("449757173");

        var student = await _studentRepository.GetBySRN(srn.Value);

        var attendancePlan = AttendancePlan.Create(student);

        var offerings = await _offeringRepository.GetByStudentId(student.Id);

        foreach (var offering in offerings)
        {
            var course = await _courseRepository.GetByOfferingId(offering.Id);

            var periodIds = offering.Sessions
                .Where(session => !session.IsDeleted)
                .Select(session => session.PeriodId)
                .ToList();

            var periods = await _periodRepository.GetListFromIds(periodIds);

            attendancePlan.AddPeriods(periods, offering, course);
        }

        List<(AttendancePlanPeriodId PeriodId, TimeOnly EntryTime, TimeOnly ExitTime)> periodUpdates = new();

        foreach (var period in attendancePlan.Periods)
        {
            if (period.StartTime == new TimeOnly(8, 55, 0))
            {
                periodUpdates.Add(new()
                {
                    PeriodId = period.Id,
                    EntryTime = new(9, 0, 0),
                    ExitTime = new(9, 45, 0)
                });
            }

            if (period.StartTime == new TimeOnly(9, 45, 0))
            {
                periodUpdates.Add(new()
                {
                    PeriodId = period.Id,
                    EntryTime = new(9, 45, 0),
                    ExitTime = new(10, 35, 0)
                });
            }

            if (period.StartTime == new TimeOnly(11, 20, 0))
            {
                periodUpdates.Add(new()
                {
                    PeriodId = period.Id,
                    EntryTime = new(11, 30, 0),
                    ExitTime = new(12, 10, 0)
                });
            }

            if (period.StartTime == new TimeOnly(12, 10, 0))
            {
                if (period.Day.Equals(PeriodDay.Wednesday))
                {
                    periodUpdates.Add(new()
                    {
                        PeriodId = period.Id,
                        EntryTime = new(12, 10, 0),
                        ExitTime = new(12, 34, 0)
                    });
                }
                else
                {
                    periodUpdates.Add(new()
                    {
                        PeriodId = period.Id,
                        EntryTime = new(12, 10, 0),
                        ExitTime = new(13, 0, 0)
                    });
                }
            }

            if (period.StartTime == new TimeOnly(14, 20, 0))
            {
                periodUpdates.Add(new()
                {
                    PeriodId = period.Id,
                    EntryTime = new(14, 20, 0),
                    ExitTime = new(15, 10, 0)
                });
            }
        }

        Result periodUpdate = attendancePlan.UpdatePeriods(periodUpdates, _currentUserService, _dateTime);

        if (periodUpdate.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), periodUpdate.Error, true)
                .Warning("Failed to update Attendance Plan Period with supplied values");

            return null;
        }

        Result scienceLessonUpdate = attendancePlan.UpdateSciencePracLesson(PeriodWeek.WeekA, PeriodDay.Wednesday, "Period 3");

        if (scienceLessonUpdate.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), scienceLessonUpdate.Error, true)
                .Warning("Failed to update Attendance Plan Period with supplied values");

            return null;
        }

        attendancePlan.AddMissedLesson("History", 60, 60);
        attendancePlan.AddFreePeriod(PeriodWeek.WeekA, PeriodDay.Monday, "6", 60, "Science flipped lesson");

        return attendancePlan;
    }

    private async Task<AttendancePlan?> LincolnPackham()
    {
        var srn = StudentReferenceNumber.Create("449946332");

        var student = await _studentRepository.GetBySRN(srn.Value);

        var attendancePlan = AttendancePlan.Create(student);

        var offerings = await _offeringRepository.GetByStudentId(student.Id);

        foreach (var offering in offerings)
        {
            var course = await _courseRepository.GetByOfferingId(offering.Id);

            var periodIds = offering.Sessions
                .Where(session => !session.IsDeleted)
                .Select(session => session.PeriodId)
                .ToList();

            var periods = await _periodRepository.GetListFromIds(periodIds);

            attendancePlan.AddPeriods(periods, offering, course);
        }

        //List<(AttendancePlanPeriodId PeriodId, TimeOnly EntryTime, TimeOnly ExitTime)> periodUpdates = new();

        //foreach (var period in attendancePlan.Periods)
        //{
        //    if (period.StartTime == new TimeOnly(8, 55, 0))
        //    {
        //        periodUpdates.Add(new()
        //        {
        //            PeriodId = period.Id,
        //            EntryTime = new(9, 0, 0),
        //            ExitTime = new(9, 45, 0)
        //        });
        //    }

        //    if (period.StartTime == new TimeOnly(9, 45, 0))
        //    {
        //        periodUpdates.Add(new()
        //        {
        //            PeriodId = period.Id,
        //            EntryTime = new(9, 45, 0),
        //            ExitTime = new(10, 35, 0)
        //        });
        //    }

        //    if (period.StartTime == new TimeOnly(11, 20, 0))
        //    {
        //        periodUpdates.Add(new()
        //        {
        //            PeriodId = period.Id,
        //            EntryTime = new(11, 20, 0),
        //            ExitTime = new(12, 10, 0)
        //        });
        //    }

        //    if (period.StartTime == new TimeOnly(12, 10, 0))
        //    {
        //        periodUpdates.Add(new()
        //        {
        //            PeriodId = period.Id,
        //            EntryTime = new(12, 10, 0),
        //            ExitTime = new(13, 0, 0)
        //        });
        //    }

        //    if (period.StartTime == new TimeOnly(14, 20, 0))
        //    {
        //        periodUpdates.Add(new()
        //        {
        //            PeriodId = period.Id,
        //            EntryTime = new(14, 20, 0),
        //            ExitTime = new(15, 10, 0)
        //        });
        //    }
        //}

        //Result periodUpdate = attendancePlan.UpdatePeriods(periodUpdates, _currentUserService, _dateTime);

        //if (periodUpdate.IsFailure)
        //{
        //    _logger
        //        .ForContext(nameof(Error), periodUpdate.Error, true)
        //        .Warning("Failed to update Attendance Plan Period with supplied values");

        //    return null;
        //}

        return attendancePlan;
    }

    public class ProjectedPercentage
    {
        public AttendancePlanId PlanId { get; set; }
        public CourseId CourseId { get; set; }
        public string Course { get; set; }
        public OfferingId OfferingId { get; set; }
        public string Class { get; set; }
        public double MinutesPresent { get; set; }
        public double Percentage { get; set; }
    }
}