namespace Constellation.Presentation.Server.Areas.Subject.Pages.SciencePracs.Lessons;

using Constellation.Application.Courses.GetCoursesForSelectionList;
using Constellation.Application.SciencePracs.GetLessonDetails;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Shared;
using Constellation.Presentation.Server.BaseModels;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

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

    }
}
