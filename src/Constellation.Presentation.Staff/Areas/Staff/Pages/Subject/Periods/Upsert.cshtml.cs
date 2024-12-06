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

    public int Day { get; set; }
    public int Period { get; set; }
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

        Day = request.Value.Day;
        Period = request.Value.Period;
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
        UpsertPeriodCommand command = new(
            Id,
            (Day / 5) + 1,
            PeriodDay.FromValue(Day % 5),
            Period,
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
}