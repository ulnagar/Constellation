namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Subject.SciencePracs.Lessons;

using Constellation.Application.Models.Auth;
using Constellation.Application.SciencePracs.GetLessonRollDetails;
using Constellation.Application.SciencePracs.SubmitRoll;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

[Authorize(Policy = AuthPolicies.CanManageSciencePracs)]
public class SubmitModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly LinkGenerator _linkGenerator;

    public SubmitModel(
        IMediator mediator,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Subject_SciencePracs_Lessons;

    [BindProperty(SupportsGet = true)]
    public Guid LessonId { get; set; }
    [BindProperty(SupportsGet = true)]
    public Guid RollId { get; set; }

    public string LessonName { get; set; }
    public DateOnly DueDate { get; set; }

    [BindProperty]
    public DateOnly LessonDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [BindProperty]
    public List<AttendanceRecord> Attendance { get; set; } = new();

    [BindProperty]
    public string Comment { get; set; }

    public async Task OnGet()
    {
        SciencePracLessonId sciencePracLessonId = SciencePracLessonId.FromValue(LessonId);
        SciencePracRollId sciencePracRollId = SciencePracRollId.FromValue(RollId);

        Result<LessonRollDetailsResponse> rollRequest = await _mediator.Send(new GetLessonRollDetailsQuery(sciencePracLessonId, sciencePracRollId));

        if (rollRequest.IsFailure)
        {
            Error = new()
            {
                Error = rollRequest.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/Subject/SciencePracs/Lessons/Roll", values: new { area = "Staff", lessonId = LessonId, rollId = RollId })
            };

            return;
        }

        var roll = rollRequest.Value;

        LessonName = roll.Name;
        DueDate = roll.DueDate;
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

    public async Task<IActionResult> OnPostSubmit()
    {
        SciencePracLessonId sciencePracLessonId = SciencePracLessonId.FromValue(LessonId);
        SciencePracRollId sciencePracRollId = SciencePracRollId.FromValue(RollId);

        if (Attendance.Count(entry => entry.Present) == 0 && string.IsNullOrWhiteSpace(Comment))
            ModelState.AddModelError("Comment", "You must enter a comment if there are no students present");

        if (LessonDate > DateOnly.FromDateTime(DateTime.Today))
            ModelState.AddModelError("LessonDate", "You cannot mark a roll for a future date.");

        if (!ModelState.IsValid)
        {
            Result<LessonRollDetailsResponse> rollRequest = await _mediator.Send(new GetLessonRollDetailsQuery(sciencePracLessonId, sciencePracRollId));

            if (rollRequest.IsFailure)
            {
                Error = new()
                {
                    Error = rollRequest.Error,
                    RedirectPath = _linkGenerator.GetPathByPage("/Subject/SciencePracs/Lessons/Roll", values: new { area = "Staff", lessonId = LessonId, rollId = RollId })
                };

                return Page();
            }

            var roll = rollRequest.Value;

            LessonName = roll.Name;
            DueDate = roll.DueDate;

            return Page();
        }

        SubmitRollCommand command = new(
            sciencePracLessonId,
            sciencePracRollId,
            LessonDate.ToDateTime(TimeOnly.MinValue),
            Comment,
            Attendance.Where(entry => entry.Present).Select(entry => entry.StudentId).ToList(),
            Attendance.Where(entry => !entry.Present).Select(entry => entry.StudentId).ToList());

        Result commandRequest = await _mediator.Send(command);

        if (commandRequest.IsFailure)
        {
            Error = new()
            {
                Error = commandRequest.Error,
                RedirectPath = null
            };

            Result<LessonRollDetailsResponse> rollRequest = await _mediator.Send(new GetLessonRollDetailsQuery(sciencePracLessonId, sciencePracRollId));

            if (rollRequest.IsFailure)
            {
                Error = new()
                {
                    Error = rollRequest.Error,
                    RedirectPath = _linkGenerator.GetPathByPage("/Subject/SciencePracs/Lessons/Roll", values: new { area = "Staff", lessonId = LessonId, rollId = RollId })
                };

                return Page();
            }

            var roll = rollRequest.Value;

            LessonName = roll.Name;
            DueDate = roll.DueDate;

            return Page();
        }

        return RedirectToPage("/Subject/SciencePracs/Lessons/Roll", new { area = "Staff", lessonId = LessonId, rollId = RollId });
    }

    public class AttendanceRecord
    {
        public SciencePracAttendanceId Id { get; set; }
        public string StudentId { get; set; }
        public string Name { get; set; }
        public bool Present { get; set; }
    }
}
