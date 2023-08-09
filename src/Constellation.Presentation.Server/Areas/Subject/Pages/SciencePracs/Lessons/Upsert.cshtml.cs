namespace Constellation.Presentation.Server.Areas.Subject.Pages.SciencePracs.Lessons;

using Constellation.Application.Courses.GetCoursesForSelectionList;
using Constellation.Application.Models.Auth;
using Constellation.Application.SciencePracs.CreateLesson;
using Constellation.Application.SciencePracs.GetLessonDetails;
using Constellation.Application.SciencePracs.UpdateLesson;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Shared;
using Constellation.Presentation.Server.BaseModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

[Authorize(Policy = AuthPolicies.CanManageSciencePracs)]
public class UpsertModel : BasePageModel
{
    private readonly IMediator _mediator;

    public UpsertModel(
        IMediator mediator)
    {
        _mediator = mediator;
    }

    [ViewData]
    public string ActivePage => "Lessons";

    [BindProperty(SupportsGet = true)]
    public Guid? Id { get; set; }

    [BindProperty]
    public string LessonName { get; set; }
    [BindProperty]
    public DateOnly DueDate { get; set; }
    [BindProperty]
    public int CourseId { get; set; }
    [BindProperty]
    public bool DoNotGenerateRolls { get; set; }

    public SelectList CourseList { get; set; }

    public async Task OnGet()
    {
        await GetClasses(_mediator);

        await BuildCourseSelectList();

        if (Id.HasValue)
        {
            SciencePracLessonId lessonId = SciencePracLessonId.FromValue(Id.Value);

            Result<LessonDetailsResponse> lessonResponse = await _mediator.Send(new GetLessonDetailsQuery(lessonId));

            if (lessonResponse.IsFailure)
            {
                Error = new()
                {
                    Error = lessonResponse.Error,
                    RedirectPath = null
                };

                return;
            }

            LessonName = lessonResponse.Value.Name;
            DueDate = lessonResponse.Value.DueDate;
        }
    }

    public async Task<IActionResult> OnPostSubmit()
    {
        SciencePracLessonId lessonId = SciencePracLessonId.FromValue(Id.Value);

        if (string.IsNullOrEmpty(LessonName))
        {
            ModelState.AddModelError("LessonName", "You must specify a name for the lesson.");
        }

        if (DueDate < DateOnly.FromDateTime(DateTime.Today))
        {
            ModelState.AddModelError("DueDate", "You must specify a future due date for the lesson.");
        }

        if (!ModelState.IsValid)
        {
            await BuildCourseSelectList();

            return Page();
        }

        if (Id.HasValue)
        {
            Result updateRequest = await _mediator.Send(new UpdateLessonCommand(lessonId, LessonName, DueDate));

            if (updateRequest.IsFailure)
            {
                Error = new()
                {
                    Error = updateRequest.Error,
                    RedirectPath = null
                };

                await BuildCourseSelectList();

                return Page();
            }

            return RedirectToPage("/SciencePracs/Lessons/Details", new { area = "Subject", id = Id.Value });
        }

        Result createRequest = await _mediator.Send(new CreateLessonCommand(LessonName, DueDate, CourseId, DoNotGenerateRolls));

        if (createRequest.IsFailure)
        {
            Error = new()
            {
                Error = createRequest.Error,
                RedirectPath = null
            };

            await BuildCourseSelectList();

            return Page();
        }

        return RedirectToPage("/SciencePracs/Lessons/Index", new { area = "Subject" });
    }

    private async Task BuildCourseSelectList()
    {
        Result<List<CourseSummaryResponse>> coursesResponse = await _mediator.Send(new GetCoursesForSelectionListQuery());

        if (coursesResponse.IsFailure)
        {
            Error = new()
            {
                Error = coursesResponse.Error,
                RedirectPath = null
            };

            return;
        }

        CourseList = new SelectList(coursesResponse.Value, "Id", "DisplayName", null, "FacultyName");
    }
}
