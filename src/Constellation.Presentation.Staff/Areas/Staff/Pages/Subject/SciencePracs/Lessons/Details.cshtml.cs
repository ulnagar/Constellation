namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Subject.SciencePracs.Lessons;

using Application.Common.PresentationModels;
using Constellation.Application.Models.Auth;
using Constellation.Application.SciencePracs.CancelLesson;
using Constellation.Application.SciencePracs.GetLessonDetails;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class DetailsModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly LinkGenerator _linkGenerator;

    public DetailsModel(
        IMediator mediator,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Subject_SciencePracs_Lessons;

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public LessonDetailsResponse Lesson { get; set; }

    public async Task OnGet()
    {
        SciencePracLessonId lessonId = SciencePracLessonId.FromValue(Id);

        Result<LessonDetailsResponse> lessonRequest = await _mediator.Send(new GetLessonDetailsQuery(lessonId));

        if (lessonRequest.IsFailure)
        {
            ModalContent = new ErrorDisplay(
                lessonRequest.Error,
                _linkGenerator.GetPathByPage("/Subjects/SciencePracs/Lessons/Index", values: new { area = "Staff" }));

            return;
        }

        Lesson = lessonRequest.Value;
    }

    public async Task<IActionResult> OnGetCancel()
    {
        SciencePracLessonId lessonId = SciencePracLessonId.FromValue(Id);

        Result cancelRequest = await _mediator.Send(new CancelLessonCommand(lessonId));

        if (cancelRequest.IsFailure)
        {
            ModalContent = new ErrorDisplay(cancelRequest.Error);

            return Page();
        }

        return RedirectToPage("/Subject/SciencePracs/Lessons/Index", new { area = "Staff" });
    }
}
