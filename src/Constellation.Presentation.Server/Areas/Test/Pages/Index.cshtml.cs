namespace Constellation.Presentation.Server.Areas.Test.Pages;

using Application.Interfaces.Repositories;
using BaseModels;
using Core.Models.Attendance;
using Core.Models.Attendance.Repositories;
using Core.Models.Offerings.Identifiers;
using Core.Models.Offerings.Repositories;
using Core.Models.Students.Repositories;
using Core.Models.Students.ValueObjects;
using Core.Models.Subjects.Identifiers;
using Core.Models.Subjects.Repositories;
using Core.Models.Timetables.Enums;
using Core.Models.Timetables.Repositories;
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
    private readonly ILogger _logger;

    public IndexModel(
        IMediator mediator,
        IStudentRepository studentRepository,
        ICourseRepository courseRepository,
        IOfferingRepository offeringRepository,
        IPeriodRepository periodRepository,
        IAttendancePlanRepository attendancePlanRepository,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _mediator = mediator;
        _studentRepository = studentRepository;
        _courseRepository = courseRepository;
        _offeringRepository = offeringRepository;
        _periodRepository = periodRepository;
        _attendancePlanRepository = attendancePlanRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public AttendancePlan Plan { get; set; }
    public List<ProjectedPercentage> Percentages { get; set; } = new();

    public async Task OnGet()
    {
        var plans = await _attendancePlanRepository.GetAll();

        Plan = plans.First();

        await ExtendPlans(Plan);
    }

    private async Task SavePlansToDb()
    {
        var leo = await LeoWoolacott();

        _attendancePlanRepository.Insert(leo);

        var mackenzie = await MackenzieJohnson();

        _attendancePlanRepository.Insert(mackenzie);

        var lincoln = await LincolnPackham();

        _attendancePlanRepository.Insert(lincoln);

        await _unitOfWork.CompleteAsync();
    }

    private async Task ExtendPlans(AttendancePlan plan)
    {
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
                CourseId = course.Id,
                Course = course.Name,
                OfferingId = offering.Id,
                Class = offering.Name,
                MinutesPresent = total,
                Percentage = (total / course.TargetMinutesPerCycle)
            });
        }
    }

    private async Task<AttendancePlan> LeoWoolacott()
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

        foreach (var period in attendancePlan.Periods)
        {
            if (period.StartTime == new TimeOnly(9, 10, 0))
            {
            }

            if (period.StartTime == new TimeOnly(11, 30, 0))
            {
                period.UpdateDetails(
                    new TimeOnly(11, 50, 0),
                    new TimeOnly(13, 10, 0));
            }

            if (period.StartTime == new TimeOnly(14, 10, 0))
            {
                period.UpdateDetails(
                    new TimeOnly(14, 10, 0),
                    new TimeOnly(15, 00, 0));
            }
        }

        var percentages = attendancePlan.Percentages;

        return attendancePlan;
    }

    private async Task<AttendancePlan> MackenzieJohnson()
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

        foreach (var period in attendancePlan.Periods)
        {
            if (period.StartTime == new TimeOnly(8, 55, 0))
            {
                period.UpdateDetails(
                    new TimeOnly(9, 0, 0),
                    new TimeOnly(9, 45, 0));
            }

            if (period.StartTime == new TimeOnly(9, 45, 0))
            {
                period.UpdateDetails(
                    new TimeOnly(9, 45, 0),
                    new TimeOnly(10, 35, 0));
            }

            if (period.StartTime == new TimeOnly(11, 20, 0))
            {
                period.UpdateDetails(
                    new TimeOnly(11, 30, 0),
                    new TimeOnly(12, 10, 0));
            }

            if (period.StartTime == new TimeOnly(12, 10, 0))
            {
                period.UpdateDetails(
                    new TimeOnly(12, 10, 0),
                    new TimeOnly(13, 0, 0));

                if (period.Day == PeriodDay.Wednesday)
                {
                    period.UpdateDetails(
                        new TimeOnly(12, 10, 0),
                        new TimeOnly(12, 34, 0));
                }
            }

            if (period.StartTime == new TimeOnly(14, 20, 0))
            {
                period.UpdateDetails(
                    new TimeOnly(14, 20, 0),
                    new TimeOnly(15, 10, 0));
            }
        }

        var percentages = attendancePlan.Percentages;

        return attendancePlan;
    }

    private async Task<AttendancePlan> LincolnPackham()
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

        foreach (var period in attendancePlan.Periods)
        {
            if (period.StartTime == new TimeOnly(8, 55, 0))
            {
                period.UpdateDetails(
                    new TimeOnly(9, 0, 0),
                    new TimeOnly(9, 45, 0));
            }

            if (period.StartTime == new TimeOnly(9, 45, 0))
            {
                period.UpdateDetails(
                    new TimeOnly(9, 45, 0),
                    new TimeOnly(10, 35, 0));
            }

            if (period.StartTime == new TimeOnly(11, 20, 0))
            {
                period.UpdateDetails(
                    new TimeOnly(11, 20, 0),
                    new TimeOnly(12, 10, 0));
            }

            if (period.StartTime == new TimeOnly(12, 10, 0))
            {
                period.UpdateDetails(
                    new TimeOnly(12, 10, 0),
                    new TimeOnly(13, 0, 0));
            }

            if (period.StartTime == new TimeOnly(14, 20, 0))
            {
                period.UpdateDetails(
                    new TimeOnly(14, 20, 0),
                    new TimeOnly(15, 10, 0));
            }
        }

        var percentages = attendancePlan.Percentages;

        return attendancePlan;
    }

    public class ProjectedPercentage
    {
        public CourseId CourseId { get; set; }
        public string Course { get; set; }
        public OfferingId OfferingId { get; set; }
        public string Class { get; set; }
        public double MinutesPresent { get; set; }
        public double Percentage { get; set; }
    }
}