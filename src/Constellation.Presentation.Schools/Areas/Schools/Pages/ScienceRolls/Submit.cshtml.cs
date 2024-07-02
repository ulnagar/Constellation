namespace Constellation.Presentation.Schools.Areas.Schools.Pages.ScienceRolls;

using Application.Common.PresentationModels;
using Application.Models.Auth;
using Application.SciencePracs.GetLessonRollSubmitContextForSchoolsPortal;
using Application.SciencePracs.SubmitRoll;
using Constellation.Core.Models.Identifiers;
using Constellation.Presentation.Shared.Helpers.ModelBinders;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.DataAnnotations;
using static Constellation.Application.SciencePracs.GetLessonRollSubmitContextForSchoolsPortal.ScienceLessonRollForSubmit;

[Authorize(Policy = AuthPolicies.IsSchoolContact)]
public class SubmitModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;

    public SubmitModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        IHttpContextAccessor httpContextAccessor, 
        IServiceScopeFactory serviceFactory) 
        : base(httpContextAccessor, serviceFactory)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

    [ViewData] public string ActivePage => Models.ActivePage.ScienceRolls;

    [BindProperty(SupportsGet = true)]
    [ModelBinder(typeof(StrongIdBinder))]
    public SciencePracLessonId LessonId { get; set; }

    [BindProperty(SupportsGet = true)]
    [ModelBinder(typeof(StrongIdBinder))]
    public SciencePracRollId RollId { get; set; }

    public string LessonName { get; set; }
    public DateTime LessonDueDate { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    [BindProperty]
    public DateTime LessonDate { get; set; } = DateTime.Today;

    [BindProperty]
    public string Comment { get; set; } = string.Empty;

    public List<StudentAttendance> Attendance { get; set; } = new();

    public async Task OnGet()
    {
        Result<ScienceLessonRollForSubmit> rollRequest = await _mediator.Send(new GetLessonRollSubmitContextForSchoolsPortalQuery(LessonId, RollId));

        if (rollRequest.IsFailure)
        {
            ModalContent = new ErrorDisplay(rollRequest.Error);

            return;
        }

        LessonName = rollRequest.Value.LessonName;
        LessonDueDate = rollRequest.Value.LessonDueDate;
        Attendance = rollRequest.Value.Attendance;
    }

    public async Task<IActionResult> OnPost()
    {
        List<string> presentStudents = Attendance
            .Where(entry => entry.Present)
            .Select(entry => entry.StudentId)
            .ToList();

        if (presentStudents.Count == 0 && (string.IsNullOrWhiteSpace(Comment) || Comment.Length < 5))
            ModelState.AddModelError(nameof(Comment), "You must provide a comment if none of the students were present");

        if (LessonDate > DateTime.Today)
            ModelState.AddModelError(nameof(LessonDate), "You cannot mark a roll for the future");

        if (!ModelState.IsValid)
        {
            return Page();
        }

        List<string> absentStudents = Attendance
            .Where(entry => !entry.Present)
            .Select(entry => entry.StudentId)
            .ToList();

        SubmitRollCommand command = new(
            LessonId,
            RollId,
            LessonDate,
            Comment,
            presentStudents,
            absentStudents);

        Result result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            ModalContent = new ErrorDisplay(result.Error);

            return Page();
        }

        return RedirectToPage("/ScienceRolls/Index", new { area = "Schools" });
    }
}