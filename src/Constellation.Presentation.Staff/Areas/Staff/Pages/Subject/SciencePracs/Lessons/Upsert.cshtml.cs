namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Subject.SciencePracs.Lessons;

using Application.Common.PresentationModels;
using Application.Domains.Courses.Queries.GetCoursesForSelectionList;
using Application.Domains.SciencePracs.Commands.CreateLesson;
using Application.Domains.SciencePracs.Commands.UpdateLesson;
using Application.Domains.SciencePracs.Queries.GetLessonDetails;
using Constellation.Application.Domains.Courses.Models;
using Constellation.Application.Models.Auth;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.Subjects.Identifiers;
using Constellation.Core.Shared;
using Core.Abstractions.Services;
using Core.Models.SciencePracs.Errors;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Helpers.Logging;
using Serilog;

[Authorize(Policy = AuthPolicies.CanManageSciencePracs)]
public class UpsertModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public UpsertModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<UpsertModel>()
            .ForContext(LogDefaults.Application, LogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Subject_SciencePracs_Lessons;
    [ViewData] public string PageTitle { get; set; } = "New Lesson";

    [BindProperty(SupportsGet = true)]
    public SciencePracLessonId Id { get; set; } = SciencePracLessonId.Empty;

    [BindProperty]
    public string LessonName { get; set; }
    [BindProperty]
    public DateOnly DueDate { get; set; }
    [BindProperty]
    public CourseId CourseId { get; set; }
    [BindProperty]
    public bool DoNotGenerateRolls { get; set; }

    public SelectList CourseList { get; set; }

    public async Task OnGet()
    {
        await BuildCourseSelectList();

        DueDate = DateOnly.FromDateTime(DateTime.Today);

        if (Id != SciencePracLessonId.Empty)
        {
            _logger.Information("Requested to retrieve Lesson with id {Id} for edit by user {User}", Id, _currentUserService.UserName);

            Result<LessonDetailsResponse> lessonResponse = await _mediator.Send(new GetLessonDetailsQuery(Id));

            if (lessonResponse.IsFailure)
            {
                _logger
                    .ForContext(nameof(Error), lessonResponse.Error, true)
                    .Warning("Failed to retrieve Lesson with id {Id} for edit by user {User}", Id, _currentUserService.UserName);

                ModalContent = new ErrorDisplay(
                    lessonResponse.Error,
                    _linkGenerator.GetPathByPage("/Subject/SciencePracs/Lessons/Details", values: new { area = "Staff", Id }));

                return;
            }

            if (lessonResponse.Value.Rolls.Any(roll => roll.Status == Core.Enums.LessonStatus.Completed))
            {
                _logger
                    .ForContext(nameof(Error), SciencePracLessonErrors.CannotEdit, true)
                    .Warning("Failed to retrieve Lesson with id {Id} for edit by user {User}", Id, _currentUserService.UserName);

                ModalContent = new ErrorDisplay(
                    SciencePracLessonErrors.CannotEdit,
                    _linkGenerator.GetPathByPage("/Subject/SciencePracs/Lessons/Details", values: new { area = "Staff", Id }));
            }

            LessonName = lessonResponse.Value.Name;
            DueDate = lessonResponse.Value.DueDate;
            CourseId = lessonResponse.Value.CourseId;
            
            PageTitle = $"Edit - {LessonName}";
        }
    }

    public async Task<IActionResult> OnPostUpdate()
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

        UpdateLessonCommand command = new(Id, LessonName, DueDate);

        _logger
            .ForContext(nameof(UpdateLessonCommand), command, true)
            .Information("Requested to update Lesson by user {User}", _currentUserService.UserName);

        Result updateRequest = await _mediator.Send(command);

        if (updateRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), updateRequest.Error, true)
                .Warning("Failed to update Lesson by user {User}", _currentUserService.UserName);

            ModalContent = new ErrorDisplay(updateRequest.Error);

            await BuildCourseSelectList();

            return Page();
        }

        return RedirectToPage("/Subject/SciencePracs/Lessons/Details", new { area = "Staff", id = Id.Value });
    }

    public async Task<IActionResult> OnPostCreate()
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

        CreateLessonCommand command = new(LessonName, DueDate, CourseId, DoNotGenerateRolls);

        _logger
            .ForContext(nameof(CreateLessonCommand), command, true)
            .Information("Requested to create new Lesson by user {User}", _currentUserService.UserName);

        Result createRequest = await _mediator.Send(command);

        if (createRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), createRequest.Error, true)
                .Warning("Failed to create new Lesson by user {User}", _currentUserService.UserName);

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
