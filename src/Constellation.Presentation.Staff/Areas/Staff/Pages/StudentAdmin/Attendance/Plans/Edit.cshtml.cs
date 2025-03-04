namespace Constellation.Presentation.Staff.Areas.Staff.Pages.StudentAdmin.Attendance.Plans;

using Application.Attendance.Plans.SaveDraftAttendancePlan;
using Application.Models.Auth;
using Constellation.Application.Attendance.Plans.CopyAttendancePlanDetails;
using Constellation.Application.Attendance.Plans.GetAttendancePlanForSubmit;
using Constellation.Application.Attendance.Plans.GetRecentlyCompletedPlans;
using Constellation.Application.Attendance.Plans.SubmitAttendancePlan;
using Constellation.Application.Common.PresentationModels;
using Constellation.Core.Models.Attendance.Identifiers;
using Constellation.Core.Models.Timetables.Enums;
using Constellation.Core.Shared;
using Constellation.Presentation.Shared.Helpers.ModelBinders;
using Core.Abstractions.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Models;
using Serilog;

[Authorize(Policy = AuthPolicies.CanManageAbsences)]
public class EditModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public EditModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<EditModel>()
            .ForContext(StaffLogDefaults.Application, StaffLogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.StudentAdmin_Attendance_Plans;
    [ViewData] public string PageTitle => "Edit Attendance Plan";

    [BindProperty(SupportsGet = true)]
    public AttendancePlanId Id { get; set; } = AttendancePlanId.Empty;

    public AttendancePlanEntry Plan { get; set; }

    public SelectList Weeks { get; set; }
    public SelectList Days { get; set; }
    public SelectList StudentPlans { get; set; }

    public async Task OnGet() => await PreparePage();

    private async Task<IActionResult> PreparePage()
    {
        Result<AttendancePlanEntry> plan = await _mediator.Send(new GetAttendancePlanForSubmitQuery(Id));

        if (plan.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), plan.Error, true)
                .Warning("Failed to retrieve Attendance Plan with id {Id} by user {User}", Id, _currentUserService.UserName);

            ModalContent = new ErrorDisplay(
                plan.Error,
                _linkGenerator.GetPathByPage("/StudentAdmin/Attendance/Plans/Index", values: new { area = "Staff" }));

            return Page();
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
                _linkGenerator.GetPathByPage("/StudentAdmin/Attendance/Plans/Index", values: new { area = "Staff" }));

            return Page();
        }

        List<CompletedPlansResponse> completedPlansList = completedPlans.Value
            .OrderBy(entry => entry.DisplayName)
            .ToList();

        Weeks = new(PeriodWeek.GetOptions, nameof(PeriodWeek.Value), nameof(PeriodWeek.Name));
        Days = new(PeriodDay.GetOptions, nameof(PeriodDay.Value), nameof(PeriodWeek.Name));
        StudentPlans = new(completedPlansList, nameof(CompletedPlansResponse.PlanId), nameof(CompletedPlansResponse.DisplayName));

        return Page();
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
                _linkGenerator.GetPathByPage("/StudentAdmin/Attendance/Plans/Details", values: new { area = "Staff", Id }));

            await PreparePage();

            return Page();
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostSave(FormData formData)
    {
        Result saveDraftAttempt = await SaveDraft(formData);

        if (saveDraftAttempt.IsFailure)
        {
            ModalContent = new ErrorDisplay(
                saveDraftAttempt.Error,
                _linkGenerator.GetPathByPage("/StudentAdmin/Attendance/Plans/Details", values: new { area = "Staff", Id }));

            return Page();
        }
        
        return RedirectToPage("/StudentAdmin/Attendance/Plans/Details", new { area = "Staff", Id });
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
                _linkGenerator.GetPathByPage("/StudentAdmin/Attendance/Plans/Details", values: new { area = "Staff", Id }));

            return Page();
        }

        Result submitAttempt = await _mediator.Send(new SubmitAttendancePlanCommand(Id));

        if (submitAttempt.IsFailure)
        {
            ModalContent = new ErrorDisplay(
                submitAttempt.Error,
                _linkGenerator.GetPathByPage("/StudentAdmin/Attendance/Plans/Details", values: new { area = "Staff", Id }));

            return Page();
        }

        return RedirectToPage("/StudentAdmin/Attendance/Plans/Index", new { area = "Staff" });
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
}