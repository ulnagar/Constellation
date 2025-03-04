namespace Constellation.Presentation.Schools.Areas.Schools.Pages.Absences.Plans;

using Application.Attendance.Plans.CopyAttendancePlanDetails;
using Application.Attendance.Plans.GetAttendancePlanForSubmit;
using Application.Attendance.Plans.GetRecentlyCompletedPlans;
using Application.Attendance.Plans.SaveDraftAttendancePlan;
using Application.Attendance.Plans.SubmitAttendancePlan;
using Application.Common.PresentationModels;
using Application.Models.Auth;
using Core.Abstractions.Services;
using Core.Models.Attendance.Identifiers;
using Core.Models.Timetables.Enums;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Presentation.Shared.Helpers.ModelBinders;
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

    public SelectList Weeks { get; set; }
    public SelectList Days { get; set; }
    public SelectList StudentPlans { get; set; }

    public PageMode Mode { get; set; } = PageMode.View;

    public async Task OnGet() => await PreparePage();

    public async Task OnGetEdit()
    {
        Mode = PageMode.Edit;

        await PreparePage();
    }

    public async Task<IActionResult> OnPostSave(FormData formData)
    {
        Result saveDraftAttempt = await SaveDraft(formData);

        if (saveDraftAttempt.IsFailure)
        {
            ModalContent = new ErrorDisplay(
                saveDraftAttempt.Error,
                _linkGenerator.GetPathByPage("/Absences/Plans/Details", values: new { area = "Schools", Id }));

            return Page();
        }

        return RedirectToPage("/Absences/Plans/Details", new { area = "Schools", Id });
    }

    public async Task<IActionResult> OnPostSubmit(FormData formData)
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

        Result saveDraftAttempt = await SaveDraft(formData);

        if (saveDraftAttempt.IsFailure)
        {
            ModalContent = new ErrorDisplay(
                saveDraftAttempt.Error,
                _linkGenerator.GetPathByPage("/Absences/Plans/Details", values: new { area = "Schools", Id }));

            return Page();
        }

        Result submitAttempt = await _mediator.Send(new SubmitAttendancePlanCommand(Id));

        if (submitAttempt.IsFailure)
        {
            ModalContent = new ErrorDisplay(
                submitAttempt.Error,
                _linkGenerator.GetPathByPage("/Absences/Plans/Details", values: new { area = "Schools", Id }));

            return Page();
        }

        return RedirectToPage("/Absences/Plans/Index", new { area = "Schools" });
    }

    public async Task<IActionResult> OnPostCopyPlan([FromBody] AttendancePlanId sourcePlanId)
    {
        CopyAttendancePlanDetailsCommand command = new(Id, sourcePlanId);

        _logger
            .ForContext(nameof(CopyAttendancePlanDetailsCommand), command, true)
            .Information("Requested to copy Attendance Plan details by user {User}", _currentUserService.UserName);

        Result operation = await _mediator.Send(new CopyAttendancePlanDetailsCommand(Id, sourcePlanId));

        if (operation.IsFailure)
        {
            _logger
                .ForContext(nameof(CopyAttendancePlanDetailsCommand), command, true)
                .ForContext(nameof(Error), operation.Error, true)
                .Warning("Failed to copy Attendance Plan details by user {User}", _currentUserService.UserName);

            ModalContent = new ErrorDisplay(
                operation.Error,
                _linkGenerator.GetPathByPage("/Absences/Plans/Details", values: new { area = "Schools", Id }));

            await PreparePage();

            return Page();
        }

        return RedirectToPage();
    }

    private async Task PreparePage()
    {
        GetAttendancePlanForSubmitQuery request = new(
            Id,
            Mode switch
            {
                PageMode.View => GetAttendancePlanForSubmitQuery.ModeOptions.View,
                PageMode.Edit => GetAttendancePlanForSubmitQuery.ModeOptions.Edit,
                _ => GetAttendancePlanForSubmitQuery.ModeOptions.View
            });

        Result<AttendancePlanEntry> plan = await _mediator.Send(request);

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

        Result<List<CompletedPlansResponse>> completedPlans = await _mediator.Send(new GetRecentlyCompletedPlansQuery(plan.Value.SchoolCode, plan.Value.Grade));

        if (completedPlans.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), plan.Error, true)
                .Warning("Failed to retrieve related Attendance Plans by user {User}", Id, _currentUserService.UserName);

            ModalContent = new ErrorDisplay(
                plan.Error,
                _linkGenerator.GetPathByPage("/Absences/Plans/Index", values: new { area = "Schools" }));

            return;
        }

        List<CompletedPlansResponse> completedPlansList = completedPlans.Value
            .OrderBy(entry => entry.DisplayName)
            .ToList();

        Weeks = new(PeriodWeek.GetOptions, nameof(PeriodWeek.Value), nameof(PeriodWeek.Name));
        Days = new(PeriodDay.GetOptions, nameof(PeriodDay.Value), nameof(PeriodWeek.Name));
        StudentPlans = new(completedPlansList, nameof(CompletedPlansResponse.PlanId), nameof(CompletedPlansResponse.DisplayName));
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

    private async Task<Result> SaveDraft(FormData formData)
    {
        List<SaveDraftAttendancePlanCommand.PlanPeriod> periodList = new();
        foreach (FormPeriod period in formData.Periods)
        {
            periodList.Add(new(period.PlanPeriodId, period.EntryTime, period.ExitTime));
        }

        SaveDraftAttendancePlanCommand.ScienceLesson? scienceLesson = (formData.ScienceLessonWeek is not null && formData.ScienceLessonDay is not null)
            ? new SaveDraftAttendancePlanCommand.ScienceLesson(formData.ScienceLessonWeek, formData.ScienceLessonDay, formData.ScienceLessonPeriod)
            : null;

        List<SaveDraftAttendancePlanCommand.FreePeriod> freePeriods = new();
        foreach (FormFreePeriods period in formData.FreePeriods)
        {
            freePeriods.Add(new(
                period.Week,
                period.Day,
                period.Period,
                period.Minutes,
                period.Activity));
        }

        List<SaveDraftAttendancePlanCommand.MissedLesson> missedLessons = new();
        foreach (FormMissedLesson missedLesson in formData.MissedLessons)
        {
            missedLessons.Add(new(
                missedLesson.Subject,
                missedLesson.TotalMinutesPerCycle,
                missedLesson.MinutesMissedPerCycle));
        }

        SaveDraftAttendancePlanCommand command = new(
            Id,
            periodList,
            scienceLesson,
            missedLessons,
            freePeriods);

        _logger
            .ForContext(nameof(SubmitAttendancePlanCommand), command, true)
            .Information("Requested to submit attendance plan by user {user}", _currentUserService.UserName);

        Result attempt = await _mediator.Send(command);

        return attempt;
    }

    public sealed class FormData
    {
        public List<FormPeriod> Periods { get; set; } = new();

        [ModelBinder(typeof(IntEnumBinder))]
        public PeriodWeek? ScienceLessonWeek { get; set; }
        [ModelBinder(typeof(IntEnumBinder))]
        public PeriodDay? ScienceLessonDay { get; set; }
        public string ScienceLessonPeriod { get; set; } = string.Empty;
        public List<FormMissedLesson> MissedLessons { get; set; } = new();
        public List<FormFreePeriods> FreePeriods { get; set; } = new();
    }

    public sealed class FormPeriod
    {
        public AttendancePlanPeriodId PlanPeriodId { get; set; }

        public TimeOnly EntryTime { get; set; }
        public TimeOnly ExitTime { get; set; }
    }

    public sealed class FormMissedLesson
    {
        public string Subject { get; set; } = string.Empty;
        public double TotalMinutesPerCycle { get; set; }
        public double MinutesMissedPerCycle { get; set; }
    }

    public sealed class FormFreePeriods
    {
        [ModelBinder(typeof(IntEnumBinder))]
        public PeriodDay Day { get; set; }
        [ModelBinder(typeof(IntEnumBinder))]
        public PeriodWeek Week { get; set; }
        public string Period { get; set; } = string.Empty;
        public double Minutes { get; set; }
        public string Activity { get; set; } = string.Empty;
    }

    public enum PageMode
    {
        View,
        Edit
    }
}