namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Subject.Periods;

using Application.Common.PresentationModels;
using Application.Models.Auth;
using Application.Periods.GetPeriodById;
using Application.Periods.UpsertPeriod;
using Core.Abstractions.Services;
using Core.Models.Timetables.Enums;
using Core.Models.Timetables.Identifiers;
using Core.Models.Timetables.ValueObjects;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Models;
using Presentation.Shared.Helpers.ModelBinders;
using Serilog;
using System.ComponentModel.DataAnnotations;

[Authorize(Policy = AuthPolicies.CanEditSubjects)]
public class UpsertModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public UpsertModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<UpsertModel>()
            .ForContext(StaffLogDefaults.Application, StaffLogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Subject_Periods_Periods;
    [ViewData] public string PageTitle { get; set; } = "New Period";

    [BindProperty(SupportsGet = true)] 
    public PeriodId Id { get; set; } = PeriodId.Empty;
    public ValidDays Day { get; set; }
    public char PeriodCode { get; set; }
    [ModelBinder(typeof(FromValueBinder))]
    public Timetable Timetable { get; set; }
    [DataType(DataType.Time)]
    public TimeSpan StartTime { get; set; }
    [DataType(DataType.Time)]
    public TimeSpan EndTime { get; set; }
    public string Name { get; set; }
    [ModelBinder(typeof(BaseFromValueBinder))]
    public PeriodType Type { get; set; }

    public SelectList Timetables { get; set; }
    public SelectList PeriodTypes { get; set; }

    public async Task OnGet()
    {
        if (Id == PeriodId.Empty)
            return;

        _logger.Information("Requested to retrieve details of Period with id {Id} for edit by user {User}", Id, _currentUserService.UserName);

        Result<PeriodResponse> request = await _mediator.Send(new GetPeriodByIdQuery(Id));

        if (request.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), request.Error, true)
                .Warning("Failed to retrieve details of Period with id {Id} for edit by user {User}", Id, _currentUserService.UserName);

            ModalContent = new ErrorDisplay(
                request.Error,
                _linkGenerator.GetPathByPage("/Subject/Periods/Index", values: new { area = "Staff"}));

            return;
        }
        
        Day = FromWeekAndDay(request.Value.Week, request.Value.Day);
        PeriodCode = request.Value.PeriodCode;
        Timetable = request.Value.Timetable;
        StartTime = request.Value.StartTime;
        EndTime = request.Value.EndTime;
        Name = request.Value.Name;
        Type = request.Value.Type;

        PageTitle = $"Edit - {Name}";

        Timetables = new SelectList(Timetable.GetEnumerable, Timetable);
        PeriodTypes = new SelectList(PeriodType.GetEnumerable, Type);
    }

    public async Task<IActionResult> OnPost()
    {
        (PeriodWeek week, PeriodDay day) convertDay = FromValidDay(Day);

        UpsertPeriodCommand command = new(
            Id,
            convertDay.week,
            convertDay.day,
            PeriodCode,
            Timetable,
            StartTime,
            EndTime,
            Name,
            Type);

        _logger
            .ForContext(nameof(UpsertPeriodCommand), command, true)
            .Information("Requested to update Period by user {User}", _currentUserService.UserName);

        Result result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Requested to update Period by user {User}", _currentUserService.UserName);

            ModalContent = new ErrorDisplay(result.Error);

            return Page();
        }

        return RedirectToPage("/Subject/Periods/Index", new { area = "Staff" });
    }

    public enum ValidDays
    {
        [Display(Name = "Week A - Monday")]
        MondayWeekA = 1,

        [Display(Name = "Week A - Tuesday")]
        TuesdayWeekA = 2,

        [Display(Name = "Week A - Wednesday")]
        WednesdayWeekA = 3,

        [Display(Name = "Week A - Thursday")]
        ThursdayWeekA = 4,

        [Display(Name = "Week A - Friday")]
        FridayWeekA = 5,

        [Display(Name = "Week B - Monday")]
        MondayWeekB = 6,

        [Display(Name = "Week B - Tuesday")]
        TuesdayWeekB = 7,

        [Display(Name = "Week B - Wednesday")]
        WednesdayWeekB = 8,

        [Display(Name = "Week B - Thursday")]
        ThursdayWeekB = 9,

        [Display(Name = "Week B - Friday")]
        FridayWeekB = 10
    }

    private (PeriodWeek week, PeriodDay day) FromValidDay(ValidDays validDay) =>
        validDay switch
        {
            ValidDays.MondayWeekA => (PeriodWeek.WeekA, PeriodDay.Monday),
            ValidDays.TuesdayWeekA => (PeriodWeek.WeekA, PeriodDay.Tuesday),
            ValidDays.WednesdayWeekA => (PeriodWeek.WeekA, PeriodDay.Wednesday),
            ValidDays.ThursdayWeekA => (PeriodWeek.WeekA, PeriodDay.Thursday),
            ValidDays.FridayWeekA => (PeriodWeek.WeekA, PeriodDay.Friday),
            ValidDays.MondayWeekB => (PeriodWeek.WeekB, PeriodDay.Monday),
            ValidDays.TuesdayWeekB => (PeriodWeek.WeekB, PeriodDay.Tuesday),
            ValidDays.WednesdayWeekB => (PeriodWeek.WeekB, PeriodDay.Wednesday),
            ValidDays.ThursdayWeekB => (PeriodWeek.WeekB, PeriodDay.Thursday),
            ValidDays.FridayWeekB => (PeriodWeek.WeekB, PeriodDay.Friday)
        };

    private ValidDays FromWeekAndDay(PeriodWeek week, PeriodDay day) =>
        (week, day) switch
        {
            (_, _) when week == PeriodWeek.WeekA && day == PeriodDay.Monday => ValidDays.MondayWeekA,
            (_, _) when week == PeriodWeek.WeekA && day == PeriodDay.Tuesday => ValidDays.TuesdayWeekA,
            (_, _) when week == PeriodWeek.WeekA && day == PeriodDay.Wednesday => ValidDays.WednesdayWeekA,
            (_, _) when week == PeriodWeek.WeekA && day == PeriodDay.Thursday => ValidDays.ThursdayWeekA,
            (_, _) when week == PeriodWeek.WeekA && day == PeriodDay.Friday => ValidDays.FridayWeekA,
            (_, _) when week == PeriodWeek.WeekB && day == PeriodDay.Monday => ValidDays.MondayWeekB,
            (_, _) when week == PeriodWeek.WeekB && day == PeriodDay.Tuesday => ValidDays.TuesdayWeekB,
            (_, _) when week == PeriodWeek.WeekB && day == PeriodDay.Wednesday => ValidDays.WednesdayWeekB,
            (_, _) when week == PeriodWeek.WeekB && day == PeriodDay.Thursday => ValidDays.ThursdayWeekB,
            (_, _) when week == PeriodWeek.WeekB && day == PeriodDay.Friday => ValidDays.FridayWeekB
        };
}