namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Subject.SciencePracs.Lessons;

using Application.Common.PresentationModels;
using Constellation.Application.Models.Auth;
using Constellation.Application.SciencePracs.GetLessonRollDetails;
using Constellation.Application.SciencePracs.SubmitRoll;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Shared;
using Core.Abstractions.Services;
using Core.Models.Students.Identifiers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Models;
using Presentation.Shared.Helpers.Logging;
using Serilog;

[Authorize(Policy = AuthPolicies.CanManageSciencePracs)]
public class SubmitModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public SubmitModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<SubmitModel>()
            .ForContext(LogDefaults.Application, LogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Subject_SciencePracs_Lessons;
    [ViewData] public string PageTitle => "Submit Lesson Roll";

    [BindProperty(SupportsGet = true)]
    public SciencePracLessonId LessonId { get; set; }
    [BindProperty(SupportsGet = true)]
    public SciencePracRollId RollId { get; set; }

    public string LessonName { get; set; }
    public DateOnly DueDate { get; set; }

    [BindProperty]
    public DateOnly LessonDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [BindProperty]
    public List<AttendanceRecord> Attendance { get; set; } = new();

    [BindProperty]
    public string? Comment { get; set; }

    public async Task OnGet() => await PreparePage();

    public async Task<IActionResult> OnPostSubmit()
    {
        if (Attendance.Count(entry => entry.Present) == 0 && string.IsNullOrWhiteSpace(Comment))
            ModelState.AddModelError("Comment", "You must enter a comment if there are no students present");

        if (LessonDate > DateOnly.FromDateTime(DateTime.Today))
            ModelState.AddModelError("LessonDate", "You cannot mark a roll for a future date.");

        if (!ModelState.IsValid)
        {
            await PreparePage();

            return Page();
        }

        SubmitRollCommand command = new(
            LessonId,
            RollId,
            LessonDate.ToDateTime(TimeOnly.MinValue),
            Comment,
            Attendance.Where(entry => entry.Present).Select(entry => entry.StudentId).ToList(),
            Attendance.Where(entry => !entry.Present).Select(entry => entry.StudentId).ToList());

        _logger
            .ForContext(nameof(SubmitRollCommand), command, true)
            .Information("Requested to submit Lesson Roll by user {User}", _currentUserService.UserName);

        Result commandRequest = await _mediator.Send(command);

        if (commandRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), commandRequest.Error, true)
                .Warning("Failed to submit Lesson Roll by user {User}", _currentUserService.UserName);

            ModalContent = new ErrorDisplay(commandRequest.Error);

            await PreparePage();

            return Page();
        }

        return RedirectToPage("/Subject/SciencePracs/Lessons/Roll", new { area = "Staff", LessonId, RollId });
    }

    private async Task PreparePage()
    {
        _logger.Information("Requested to retrieve details of Lesson Roll with id {Id} for submit by user {User}", RollId, _currentUserService.UserName);

        Result<LessonRollDetailsResponse> rollRequest = await _mediator.Send(new GetLessonRollDetailsQuery(LessonId, RollId));

        if (rollRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), rollRequest.Error, true)
                .Warning("Failed to retrieve details of Lesson Roll with id {Id} for submit by user {User}", RollId, _currentUserService.UserName);

            ModalContent = new ErrorDisplay(
                rollRequest.Error,
                _linkGenerator.GetPathByPage("/Subject/SciencePracs/Lessons/Roll", values: new { area = "Staff", LessonId, RollId }));

            return;
        }

        LessonRollDetailsResponse roll = rollRequest.Value;

        LessonName = roll.Name;
        DueDate = roll.DueDate;

        if (Attendance.Any()) return;

        foreach (LessonRollDetailsResponse.AttendanceRecord entry in roll.Attendance)
        {
            Attendance.Add(new()
            {
                Id = entry.AttendanceId,
                StudentId = entry.StudentId,
                Name = entry.StudentName.DisplayName
            });
        }
    }

    public class AttendanceRecord
    {
        public SciencePracAttendanceId? Id { get; set; }
        public StudentId StudentId { get; set; }
        public string? Name { get; set; }
        public bool Present { get; set; }
    }
}
