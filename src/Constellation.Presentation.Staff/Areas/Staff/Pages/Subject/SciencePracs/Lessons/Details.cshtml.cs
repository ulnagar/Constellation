namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Subject.SciencePracs.Lessons;

using Application.Common.PresentationModels;
using Constellation.Application.Models.Auth;
using Constellation.Application.SciencePracs.CancelLesson;
using Constellation.Application.SciencePracs.GetLessonDetails;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Shared;
using Core.Abstractions.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Models;
using Serilog;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
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
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<DetailsModel>()
            .ForContext(StaffLogDefaults.Application, StaffLogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Subject_SciencePracs_Lessons;
    [ViewData] public string PageTitle { get; set; } = "Lesson Details";

    [BindProperty(SupportsGet = true)]
    public SciencePracLessonId Id { get; set; }

    public LessonDetailsResponse Lesson { get; set; }

    public async Task OnGet() => await PreparePage();

    public async Task<IActionResult> OnGetCancel()
    {
        CancelLessonCommand command = new(Id);

        _logger
            .ForContext(nameof(CancelLessonCommand), command, true)
            .Information("Requested to cancel Lesson by user {User}", _currentUserService.UserName);

        Result cancelRequest = await _mediator.Send(command);

        if (cancelRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), cancelRequest.Error, true)
                .Warning("Failed to cancel Lesson by user {User}", _currentUserService.UserName);

            ModalContent = new ErrorDisplay(cancelRequest.Error);

            await PreparePage();

            return Page();
        }

        return RedirectToPage("/Subject/SciencePracs/Lessons/Index", new { area = "Staff" });
    }

    private async Task PreparePage()
    {
        _logger.Information("Requested to retrieve details of Lesson with id {Id} by user {User}", Id, _currentUserService.UserName);

        Result<LessonDetailsResponse> lessonRequest = await _mediator.Send(new GetLessonDetailsQuery(Id));

        if (lessonRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), lessonRequest.Error, true)
                .Warning("Failed to retrieve details of Lesson with id {Id} by user {User}", Id, _currentUserService.UserName);

            ModalContent = new ErrorDisplay(
                lessonRequest.Error,
                _linkGenerator.GetPathByPage("/Subjects/SciencePracs/Lessons/Index", values: new { area = "Staff" }));

            return;
        }

        Lesson = lessonRequest.Value;
        PageTitle = $"Details - {Lesson.Name}";
    }
}
