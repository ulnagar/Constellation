namespace Constellation.Presentation.Schools.Areas.Schools.Pages.Absences.Plans;

using Application.Attendance.Plans.GetAttendancePlanForSubmit;
using Application.Attendance.Plans.SubmitAttendancePlan;
using Application.Common.PresentationModels;
using Application.Models.Auth;
using Core.Abstractions.Services;
using Core.Models.Attendance.Identifiers;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

[Authorize(Policy = AuthPolicies.IsSchoolContact)]
public class DetailsModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public DetailsModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger,
        IHttpContextAccessor httpContextAccessor, 
        IServiceScopeFactory serviceFactory) 
        : base(httpContextAccessor, serviceFactory)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<DetailsModel>()
            .ForContext("APPLICATION", "Schools Portal");
    }

    [ViewData] public string ActivePage => Models.ActivePage.Absences;

    [BindProperty(SupportsGet = true)]
    public AttendancePlanId Id { get; set; } = AttendancePlanId.Empty;

    public AttendancePlanEntry Plan { get; set; }

    public async Task OnGet() => await PreparePage();

    public async Task<IActionResult> OnPost(List<FormPeriod> periods)
    {
        if (!ModelState.IsValid)
        {
            ModalContent = new FeedbackDisplay(
                "Form Error",
                "You must select an Entry Time and Exit Time for each period",
                "Ok",
                "btn-warning");

            await PreparePage();

            return Page();
        }

        List<SubmitAttendancePlanCommand.PlanPeriod> periodList = new();

        foreach (FormPeriod period in periods)
        {
            periodList.Add(new(period.PlanPeriodId, period.EntryTime, period.ExitTime));
        }

        SubmitAttendancePlanCommand command = new(
            Id,
            periodList);

        _logger
            .ForContext(nameof(SubmitAttendancePlanCommand), command, true)
            .Information("Requested to submit attendance plan by user {user}", _currentUserService.UserName);

        Result attempt = await _mediator.Send(command);

        if (attempt.IsFailure)
        {
            ModalContent = new ErrorDisplay(
                attempt.Error,
                _linkGenerator.GetPathByPage("/Absences/Plans/Details", values: new { area = "Schools", Id }));

            return Page();
        }

        return RedirectToPage("/Absences/Plans/Index", new { area = "Schools" });
    }

    private async Task PreparePage()
    {
        Result<AttendancePlanEntry> plan = await _mediator.Send(new GetAttendancePlanForSubmitQuery(Id));

        if (plan.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), plan.Error, true)
                .Warning("Failed to retrieve Attendance Plan with id {Id} by user {User}", Id, _currentUserService.UserName);

            ModalContent = new ErrorDisplay(
                plan.Error,
                _linkGenerator.GetPathByPage("/Absences/Plans/Index", values: new { area = "Schools" }));

            return;
        }

        Plan = plan.Value;
    }

    internal List<TimeOnly> CalculateOptions(TimeOnly start, TimeOnly end)
    {
        var response = new List<TimeOnly>();

        TimeOnly current = start;

        while (current <= end)
        {
            response.Add(current);

            current = current.AddMinutes(1);
        }

        return response;
    }

    public sealed class FormPeriod
    {
        public AttendancePlanPeriodId PlanPeriodId { get; set; }

        public TimeOnly EntryTime { get; set; }
        public TimeOnly ExitTime { get; set; }
    }
}