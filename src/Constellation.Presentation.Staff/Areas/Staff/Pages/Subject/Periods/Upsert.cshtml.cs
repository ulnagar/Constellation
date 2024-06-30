namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Subject.Periods;

using Application.Models.Auth;
using Application.Periods.GetPeriodById;
using Application.Periods.UpsertPeriod;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System.ComponentModel.DataAnnotations;

[Authorize(Policy = AuthPolicies.CanEditSubjects)]
public class UpsertModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;

    public UpsertModel(
        ISender mediator,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Subject_Periods_Periods;

    [BindProperty(SupportsGet = true)]
    public int? Id { get; set; }

    public int Day { get; set; }
    public int Period { get; set; }
    public string Timetable { get; set; }
    [DataType(DataType.Time)]
    public TimeSpan StartTime { get; set; }
    [DataType(DataType.Time)]
    public TimeSpan EndTime { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }

    public async Task OnGet()
    {
        if (Id is null)
            return;

        Result<PeriodResponse> request = await _mediator.Send(new GetPeriodByIdQuery(Id.Value));

        if (request.IsFailure)
        {
            Error = new()
            {
                Error = request.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/Subject/Periods/Index", values: new { area = "Staff"})
            };

            return;
        }

        Day = request.Value.Day;
        Period = request.Value.Period;
        Timetable = request.Value.Timetable;
        StartTime = request.Value.StartTime;
        EndTime = request.Value.EndTime;
        Name = request.Value.Name;
        Type = request.Value.Type;
    }

    public async Task<IActionResult> OnPost()
    {
        UpsertPeriodCommand command = new(
            Id,
            Day,
            Period,
            Timetable,
            StartTime,
            EndTime,
            Name,
            Type);

        Result result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            Error = new()
            {
                Error = result.Error,
                RedirectPath = null
            };

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