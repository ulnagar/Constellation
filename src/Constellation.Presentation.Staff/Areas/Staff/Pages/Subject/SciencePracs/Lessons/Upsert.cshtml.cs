namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Subject.SciencePracs.Lessons;

using Application.Common.PresentationModels;
using Constellation.Application.Courses.GetCoursesForSelectionList;
using Constellation.Application.Models.Auth;
using Constellation.Application.SciencePracs.CreateLesson;
using Constellation.Application.SciencePracs.GetLessonDetails;
using Constellation.Application.SciencePracs.UpdateLesson;
using Constellation.Core.Errors;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.Subjects.Identifiers;
using Constellation.Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;

[Authorize(Policy = AuthPolicies.CanManageSciencePracs)]
public class UpsertModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly LinkGenerator _linkGenerator;

    public UpsertModel(
        IMediator mediator,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Subject_SciencePracs_Lessons;

    [BindProperty(SupportsGet = true)]
    public Guid? Id { get; set; }

    [BindProperty]
    public string LessonName { get; set; }
    [BindProperty]
    public DateOnly DueDate { get; set; }
    [BindProperty]
    public Guid CourseId { get; set; }
    [BindProperty]
    public bool DoNotGenerateRolls { get; set; }

    public SelectList CourseList { get; set; }

    public async Task OnGet()
    {
        await BuildCourseSelectList();

        DueDate = DateOnly.FromDateTime(DateTime.Today);

        if (Id.HasValue)
        {
            SciencePracLessonId lessonId = SciencePracLessonId.FromValue(Id.Value);

            Result<LessonDetailsResponse> lessonResponse = await _mediator.Send(new GetLessonDetailsQuery(lessonId));

            if (lessonResponse.IsFailure)
            {
                ModalContent = new ErrorDisplay(
                    lessonResponse.Error,
                    _linkGenerator.GetPathByPage("/Subject/SciencePracs/Lessons/Details", values: new { area = "Staff", id = Id }));

                return;
            }

            if (lessonResponse.Value.Rolls.Any(roll => roll.Status == Core.Enums.LessonStatus.Completed))
            {
                ModalContent = new ErrorDisplay(
                    DomainErrors.SciencePracs.Lesson.CannotEdit,
                    _linkGenerator.GetPathByPage("/Subject/SciencePracs/Lessons/Details", values: new { area = "Staff", id = Id }));
            }

            LessonName = lessonResponse.Value.Name;
            DueDate = lessonResponse.Value.DueDate;
            CourseId = lessonResponse.Value.CourseId.Value;
        }
    }

    public async Task<IActionResult> OnPostSubmit()
    {
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
            SciencePracLessonId lessonId = SciencePracLessonId.FromValue(Id.Value);

            Result updateRequest = await _mediator.Send(new UpdateLessonCommand(lessonId, LessonName, DueDate));

            if (updateRequest.IsFailure)
            {
                ModalContent = new ErrorDisplay(updateRequest.Error);

                await BuildCourseSelectList();

                return Page();
            }

            return RedirectToPage("/Subject/SciencePracs/Lessons/Details", new { area = "Staff", id = Id.Value });
        }

        CourseId courseId = Core.Models.Subjects.Identifiers.CourseId.FromValue(CourseId);

        Result createRequest = await _mediator.Send(new CreateLessonCommand(LessonName, DueDate, courseId, DoNotGenerateRolls));

        if (createRequest.IsFailure)
        {
            ModalContent = new ErrorDisplay(createRequest.Error);

            await BuildCourseSelectList();

            return Page();
        }

        return RedirectToPage("/Subject/SciencePracs/Lessons/Index", new { area = "Staff" });
    }

    private async Task BuildCourseSelectList()
    {
        Result<List<CourseSelectListItemResponse>> coursesResponse = await _mediator.Send(new GetCoursesForSelectionListQuery());

        if (coursesResponse.IsFailure)
        {
            ModalContent = new ErrorDisplay(coursesResponse.Error);

            return;
        }

        CourseList = new SelectList(coursesResponse.Value, "Id", "DisplayName", null, "FacultyName");
    }
}
