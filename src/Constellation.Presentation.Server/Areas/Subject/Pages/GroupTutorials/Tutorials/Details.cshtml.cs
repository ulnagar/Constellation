namespace Constellation.Presentation.Server.Areas.Subject.Pages.GroupTutorials.Tutorials;

using Constellation.Application.GroupTutorials.GetTutorialWithDetailsById;
using Constellation.Core.Primitives;
using Constellation.Presentation.Server.BaseModels;
using Constellation.Presentation.Server.Pages.Shared.Components.StaffTrainingReport;
using Constellation.Presentation.Server.Pages.Shared.Components.TutorialTeacherAssignment;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

public class DetailsModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly LinkGenerator _linkGenerator;

    public DetailsModel(IMediator mediator, LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    public TutorialTeacherAssignmentSelection TeacherAssignment { get; set; }

    public GroupTutorialDetailResponse Tutorial { get; set; }

    public async Task<IActionResult> OnGet(CancellationToken cancellationToken)
    {
        await GetClasses(_mediator);

        var result = await _mediator.Send(new GetTutorialWithDetailsByIdQuery(Id), cancellationToken);

        if (result.IsFailure)
        {
            Error = new ErrorDisplay
            {
                Error = result.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/GroupTutorials/Tutorials/Index", values: new { area = "Subject" })
            };

            Tutorial = new(
                Id,
                null,
                DateOnly.MinValue,
                DateOnly.MinValue,
                new List<TutorialTeacherResponse>(),
                new List<TutorialEnrolmentResponse>(),
                new List<TutorialRollResponse>());

            return Page();
        }

        Tutorial = result.Value;

        return Page();
    }

    public async Task<IActionResult> OnPostAssignTeacher()
    {
        return RedirectToPage("");
    }
}
